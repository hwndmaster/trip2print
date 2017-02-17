using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core;
using TripToPrint.Presenters;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Tests
{
    [TestClass]
    public class StepAdjustmentPresenterTests
    {
        private readonly Mock<IStepAdjustmentView> _viewMock = new Mock<IStepAdjustmentView>();
        private readonly Mock<IDialogService> _dialogServiceMock = new Mock<IDialogService>();
        private readonly Mock<IResourceNameProvider> _resourceNameMock = new Mock<IResourceNameProvider>();
        private readonly Mock<IReportGenerator> _reportGeneratorMock = new Mock<IReportGenerator>();

        private Mock<StepAdjustmentPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepAdjustmentPresenter>(
                _dialogServiceMock.Object,
                _resourceNameMock.Object,
                _reportGeneratorMock.Object) {
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
        public void When_step_is_activated_the_report_preview_is_set()
        {
            // Arrange
            _presenter.SetupGet(x => x.View).Returns(_viewMock.Object);
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepAdjustmentViewModel {
                TempPath = "temp-path"
            });
            _resourceNameMock.Setup(x => x.GetDefaultHtmlReportName()).Returns("default");

            // Act
            _presenter.Object.Activated().GetAwaiter().GetResult();

            // Verify
            _viewMock.Verify(x => x.SetAddress(@"temp-path\default"), Times.Once);
        }

        [TestMethod]
        public void When_going_next_the_user_is_asked_to_select_output_file_and_pdf_is_generated()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepAdjustmentViewModel {
                InputUri = "input.kmz",
                TempPath = "temp-path"
            });
            _dialogServiceMock.Setup(x => x.AskUserToSaveFile(It.IsAny<string>(), "input.pdf", It.IsAny<string[]>()))
                .Returns("output-filename.pdf");

            // Act
            var result = _presenter.Object.BeforeGoNext().GetAwaiter().GetResult();

            // Verify
            Assert.AreEqual(false, result);
            _reportGeneratorMock.Verify(x => x.SaveHtmlReportAsPdf("temp-path", "output-filename.pdf"));
        }
    }
}
