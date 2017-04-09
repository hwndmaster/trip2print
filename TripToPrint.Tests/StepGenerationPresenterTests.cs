using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;
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
        private readonly Mock<IProgressTrackerFactory> _progressTrackerFactoryMock = new Mock<IProgressTrackerFactory>();
        private readonly Mock<IMainWindowPresenter> _mainWindowMock = new Mock<IMainWindowPresenter>();
        private readonly Mock<IUserSession> _userSessionMock = new Mock<IUserSession>();

        private Mock<StepGenerationPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepGenerationPresenter>(
                _reportGeneratorMock.Object,
                _logStorageMock.Object,
                _loggerMock.Object,
                _progressTrackerFactoryMock.Object,
                _userSessionMock.Object) {
                CallBase = true
            };

            _presenter.SetupGet(x => x.MainWindow).Returns(_mainWindowMock.Object);

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
        public async Task When_step_is_activated_with_local_file_is_provided_the_report_generation_process_is_started()
        {
            // Arrange
            var document = new KmlDocument();
            _userSessionMock.SetupGet(x => x.Document).Returns(document);
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel());

            // Act
            await _presenter.Object.Activated();

            // Verify
            _reportGeneratorMock.Verify(x => x.Generate(document, It.IsAny<IProgressTracker>()), Times.Once);
        }

        [TestMethod]
        public async Task When_step_is_activated_with_input_filename_provided_the_report_generation_process_is_started()
        {
            // Arrange
            var document = new KmlDocument();
            _userSessionMock.SetupGet(x => x.Document).Returns(document);
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel());

            // Act
            await _presenter.Object.Activated();

            // Verify
            _reportGeneratorMock.Verify(x => x.Generate(document, It.IsAny<IProgressTracker>()), Times.Once);
        }

        [TestMethod]
        public async Task When_step_is_activated_and_report_generation_has_failed_an_error_is_put_to_log()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel());
            _reportGeneratorMock.Setup(x => x.Generate(It.IsAny<KmlDocument>(), It.IsAny<IProgressTracker>()))
                .Throws(new Exception("exception-message"));

            // Act
            await _presenter.Object.Activated();

            // Verify
            _loggerMock.Verify(x => x.Error(It.IsRegex("exception-message")));
        }

        [TestMethod]
        public async Task When_process_is_done_the_validation_to_go_next_is_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel { ProgressInPercentage = 100 });

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task When_process_is_not_done_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel { ProgressInPercentage = 99 });

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
        }
    }
}
