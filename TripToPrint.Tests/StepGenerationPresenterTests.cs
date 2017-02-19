using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.Presenters;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Tests
{
    [TestClass]
    public class StepGenerationPresenterTests
    {
        private readonly Mock<IStepGenerationView> _viewMock = new Mock<IStepGenerationView>();
        private readonly Mock<IReportGenerator> _reportGeneratorMock = new Mock<IReportGenerator>();
        private readonly Mock<ILogStorage> _logStorageMock = new Mock<ILogStorage>();
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();
        private readonly Mock<IWebClientService> _webClientkMock = new Mock<IWebClientService>();
        private readonly Mock<IFileService> _fileMock = new Mock<IFileService>();
        private readonly Mock<IGoogleMyMapAdapter> _googleMyMapAdapterMock = new Mock<IGoogleMyMapAdapter>();
        private readonly Mock<IResourceNameProvider> _resourceNameMock = new Mock<IResourceNameProvider>();
        private readonly Mock<IProgressTrackerFactory> _progressTrackerFactoryMock = new Mock<IProgressTrackerFactory>();

        private Mock<StepGenerationPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepGenerationPresenter>(
                _reportGeneratorMock.Object,
                _logStorageMock.Object,
                _loggerMock.Object,
                _webClientkMock.Object,
                _fileMock.Object,
                _googleMyMapAdapterMock.Object,
                _resourceNameMock.Object,
                _progressTrackerFactoryMock.Object) {
                CallBase = true
            };

            _progressTrackerFactoryMock.Setup(x => x.Create(It.IsAny<Action<int>>()))
                .Returns(new Mock<IProgressTracker>().Object);
        }

        [TestMethod]
        public void When_initializing_presenter_the_properties_are_set_correctly()
        {
            // Act
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Verify
            Assert.AreEqual(_viewMock.Object, _presenter.Object.View);
            Assert.IsNotNull(_presenter.Object.ViewModel);
            _viewMock.VerifySet(x => x.Presenter = _presenter.Object);
            _viewMock.VerifySet(x => x.DataContext = _presenter.Object.ViewModel);
        }

        [TestMethod]
        public void When_step_is_activated_with_local_file_is_provided_the_report_generation_process_is_started()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel {
                InputUri = "input-uri"
            });

            // Act
            _presenter.Object.Activated().GetAwaiter().GetResult();

            // Verify
            _reportGeneratorMock.Verify(x => x.Generate("input-uri", It.IsAny<IProgressTracker>()), Times.Once);
        }

        [TestMethod]
        public void When_step_is_activated_with_url_provided_the_kml_downloaded_and_report_generation_process_is_started()
        {
            // Arrange
            var kmzUrl = new Uri("http://kml-uri");
            var folderPrefix = "folder-prefix";
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel
            {
                InputSource = InputSource.GoogleMyMapsUrl,
                InputUri = "http://input-uri"
            });
            _googleMyMapAdapterMock.Setup(x => x.GetKmlDownloadUrl(new Uri("http://input-uri"))).Returns(kmzUrl);
            _resourceNameMock.Setup(x => x.GetTempFolderPrefix()).Returns(folderPrefix);

            // Act
            _presenter.Object.Activated().GetAwaiter().GetResult();

            // Verify
            _webClientkMock.Verify(x => x.GetAsync(kmzUrl), Times.Once);
            _reportGeneratorMock.Verify(x => x.Generate(
                It.IsRegex($@"{Path.GetTempPath().Replace(@"\", @"\\")}{folderPrefix}[\w\-]{{36}}\.kmz"),
                It.IsAny<IProgressTracker>()), Times.Once);
        }

        [TestMethod]
        public void When_step_is_activated_with_invalid_url_provided_an_error_is_put_to_log()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel
            {
                InputSource = InputSource.GoogleMyMapsUrl,
                InputUri = "http://input-uri"
            });

            // Act
            _presenter.Object.Activated().GetAwaiter().GetResult();

            // Verify
            _loggerMock.Verify(x => x.Error(It.IsRegex("InvalidOperationException")));
        }

        [TestMethod]
        public void When_step_is_activated_with_not_accessible_url_provided_an_error_is_put_to_log()
        {
            // Arrange
            var kmzUrl = new Uri("http://kml-uri");
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel
            {
                InputSource = InputSource.GoogleMyMapsUrl,
                InputUri = "http://input-uri"
            });
            _googleMyMapAdapterMock.Setup(x => x.GetKmlDownloadUrl(new Uri("http://input-uri"))).Returns(kmzUrl);
            _webClientkMock.Setup(x => x.GetAsync(kmzUrl)).Throws<Exception>();

            // Act
            _presenter.Object.Activated().GetAwaiter().GetResult();

            // Verify
            _loggerMock.Verify(x => x.Error(It.IsRegex("InvalidOperationException")));
        }

        [TestMethod]
        public void When_step_is_activated_and_report_generation_has_failed_an_error_is_put_to_log_and_temporary_file_is_removed()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel {
                InputSource = InputSource.GoogleMyMapsUrl,
                InputUri = "http://input-uri"
            });
            _googleMyMapAdapterMock.Setup(x => x.GetKmlDownloadUrl(new Uri("http://input-uri"))).Returns(new Uri("http://kmz-uri"));
            _fileMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            _reportGeneratorMock.Setup(x => x.Generate(It.IsAny<string>(), It.IsAny<IProgressTracker>()))
                .Throws(new Exception("exception-message"));

            // Act
            _presenter.Object.Activated().GetAwaiter().GetResult();

            // Verify
            _loggerMock.Verify(x => x.Error(It.IsRegex("exception-message")));
            _fileMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void When_process_is_done_the_validation_to_go_next_is_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel { ProgressInPercentage = 100 });

            // Act
            var result = _presenter.Object.BeforeGoNext().GetAwaiter().GetResult();

            // Verify
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void When_process_is_not_done_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel { ProgressInPercentage = 99 });

            // Act
            var result = _presenter.Object.BeforeGoNext().GetAwaiter().GetResult();

            // Verify
            Assert.AreEqual(false, result);
        }
    }
}
