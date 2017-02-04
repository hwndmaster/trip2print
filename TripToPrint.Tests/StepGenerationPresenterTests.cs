using System;

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
        private Mock<StepGenerationPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepGenerationPresenter>(_reportGeneratorMock.Object, _logStorageMock.Object, _loggerMock.Object) {
                CallBase = true
            };
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
        public void When_step_is_activated_the_report_generation_process_is_started()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel {
                InputUri = "input-uri",
                OutputFileName = "output-filename"
            });

            // Act
            _presenter.Object.Activated().GetAwaiter().GetResult();

            // Verify
            _reportGeneratorMock.Verify(x => x.Generate("input-uri", "output-filename", It.IsAny<IProgressTracker>()), Times.Once);
        }

        [TestMethod]
        public void When_step_is_activated_and_report_generation_has_failed_an_error_is_put_to_log()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel {});
            _reportGeneratorMock.Setup(x => x.Generate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IProgressTracker>()))
                .Throws(new Exception("exception-message"));

            // Act
            _presenter.Object.Activated().GetAwaiter().GetResult();

            // Verify
            _loggerMock.Verify(x => x.Error("exception-message"));
        }

        [TestMethod]
        public void When_process_is_done_the_validation_to_go_next_is_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel { ProgressInPercentage = 100 });

            // Act
            var result = _presenter.Object.ValidateToGoNext();

            // Verify
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void When_process_is_not_done_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepGenerationViewModel { ProgressInPercentage = 99 });

            // Act
            var result = _presenter.Object.ValidateToGoNext();

            // Verify
            Assert.AreEqual(false, result);
        }
    }
}
