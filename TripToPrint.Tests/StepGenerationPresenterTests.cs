using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;
using TripToPrint.Core.ProgressTracking;
using TripToPrint.Presenters;
using TripToPrint.ViewModels;

namespace TripToPrint.Tests
{
    [TestClass]
    public class StepGenerationPresenterTests
    {
        private readonly Mock<IReportResourceFetcher> _reportResourceFetcherMock = new Mock<IReportResourceFetcher>();
        private readonly Mock<ILogStorage> _logStorageMock = new Mock<ILogStorage>();
        private readonly Mock<IResourceFetchingLogger> _loggerMock = new Mock<IResourceFetchingLogger>();
        private readonly Mock<IProgressTrackerFactory> _progressTrackerFactoryMock = new Mock<IProgressTrackerFactory>();
        private readonly Mock<IMainWindowPresenter> _mainWindowMock = new Mock<IMainWindowPresenter>();
        private readonly Mock<IUserSession> _userSessionMock = new Mock<IUserSession>();

        private Mock<StepGenerationPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepGenerationPresenter>(
                _reportResourceFetcherMock.Object,
                _logStorageMock.Object,
                _loggerMock.Object,
                _progressTrackerFactoryMock.Object,
                _userSessionMock.Object) {
                CallBase = true
            };

            _presenter.SetupGet(x => x.MainWindow).Returns(_mainWindowMock.Object);

            _progressTrackerFactoryMock.Setup(x => x.CreateForResourceFetching(It.IsAny<Action<int>>()))
                .Returns(new Mock<IResourceFetchingProgress>().Object);
        }

        [TestMethod]
        public async Task When_step_is_activated_with_local_file_is_provided_the_report_generation_process_is_started()
        {
            // Arrange
            var document = new KmlDocument();
            _userSessionMock.SetupGet(x => x.Document).Returns(document);
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepInProgressViewModel());

            // Act
            await _presenter.Object.Activated();

            // Verify
            _reportResourceFetcherMock.Verify(x => x.Generate(document, null, null, It.IsAny<IResourceFetchingProgress>()), Times.Once);
        }

        [TestMethod]
        public async Task When_step_is_activated_with_input_filename_provided_the_report_generation_process_is_started()
        {
            // Arrange
            var document = new KmlDocument();
            _userSessionMock.SetupGet(x => x.Document).Returns(document);
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepInProgressViewModel());

            // Act
            await _presenter.Object.Activated();

            // Verify
            _reportResourceFetcherMock.Verify(x => x.Generate(document, null, null, It.IsAny<IResourceFetchingProgress>()), Times.Once);
        }

        [TestMethod]
        public async Task When_step_is_activated_and_report_generation_has_failed_an_error_is_put_to_log()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepInProgressViewModel());
            _reportResourceFetcherMock.Setup(x => x.Generate(It.IsAny<KmlDocument>(), null, null, It.IsAny<IResourceFetchingProgress>()))
                .Throws(new Exception("exception-message"));

            // Act
            await _presenter.Object.Activated();

            // Verify
            _loggerMock.Verify(x => x.Error(It.IsRegex("exception-message")));
        }
    }
}
