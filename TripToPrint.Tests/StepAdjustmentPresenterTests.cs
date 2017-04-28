using System.Threading.Tasks;

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
        private readonly Mock<IFileService> _fileServiceMock = new Mock<IFileService>();
        private readonly Mock<IUserSession> _userSessionMock = new Mock<IUserSession>();
        private readonly Mock<IAdjustBrowserViewPresenter> _adjustBrowserViewPresenterMock = new Mock<IAdjustBrowserViewPresenter>();

        private Mock<StepAdjustmentPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepAdjustmentPresenter>(
                _dialogServiceMock.Object,
                _resourceNameMock.Object,
                _reportGeneratorMock.Object,
                _fileServiceMock.Object,
                _userSessionMock.Object,
                _adjustBrowserViewPresenterMock.Object) {
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
        public async Task When_step_is_activated_the_report_preview_is_set()
        {
            // Arrange
            var vm = CreateViewModel();
            _userSessionMock.SetupGet(x => x.GeneratedReportTempPath).Returns("temp-path");
            _resourceNameMock.Setup(x => x.GetDefaultHtmlReportName()).Returns("default");

            // Act
            await _presenter.Object.Activated();

            // Verify
            Assert.AreEqual(@"temp-path\default", vm.AdjustBrowser.Address);
        }

        [TestMethod]
        public async Task When_going_next_the_user_is_asked_to_select_output_file_and_pdf_is_generated()
        {
            // Arrange
            var vm = CreateViewModel();
            _userSessionMock.SetupGet(x => x.InputUri).Returns("input.kmz");
            _userSessionMock.SetupGet(x => x.GeneratedReportTempPath).Returns("temp-path");
            SetupDialogServiceAskUserToSaveFile("input.pdf", "output-filename.pdf");
            _reportGeneratorMock.Setup(x => x.SaveHtmlReportAsPdf("temp-path", "output-filename.pdf"))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
            _reportGeneratorMock.Verify(x => x.SaveHtmlReportAsPdf("temp-path", "output-filename.pdf"));
            Assert.AreEqual("output-filename.pdf", vm.OutputFilePath);
        }

        [TestMethod]
        public async Task When_going_next_and_user_is_asked_to_select_output_file_but_cancelled_the_report_isnt_generated()
        {
            // Arrange
            var vm = CreateViewModel(outputFilePath: "output-filepath");
            _userSessionMock.SetupGet(x => x.InputUri).Returns("input.kmz");
            _userSessionMock.SetupGet(x => x.GeneratedReportTempPath).Returns("temp-path");
            SetupDialogServiceAskUserToSaveFile("input.pdf", null);

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
            _reportGeneratorMock.Verify(x => x.SaveHtmlReportAsPdf("temp-path", "output-filename.pdf"), Times.Never);
            Assert.AreEqual(null, vm.OutputFilePath);
        }

        [TestMethod]
        public async Task When_going_next_and_saving_to_pdf_has_failed_an_error_message_is_displayed()
        {
            // Arrange
            var vm = CreateViewModel();
            _userSessionMock.SetupGet(x => x.InputUri).Returns("input.kmz");
            _userSessionMock.SetupGet(x => x.GeneratedReportTempPath).Returns("temp-path");
            SetupDialogServiceAskUserToSaveFile("input.pdf", "output-filename.pdf");
            _reportGeneratorMock.Setup(x => x.SaveHtmlReportAsPdf("temp-path", "output-filename.pdf"))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
            _dialogServiceMock.Verify(x => x.InvalidOperationMessage(It.IsAny<string>()));
            Assert.AreEqual(null, vm.OutputFilePath);
        }

        [TestMethod]
        public async Task When_going_back_the_output_filepath_is_cleared()
        {
            // Arrange
            var vm = CreateViewModel(outputFilePath: "something");

            // Act
            await _presenter.Object.BeforeGoBack();

            // Verify
            Assert.AreEqual(null, vm.OutputFilePath);
        }

        private StepAdjustmentViewModel CreateViewModel(string outputFilePath = null)
        {
            var vm = new StepAdjustmentViewModel {
                OutputFilePath = outputFilePath
            };
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            return vm;
        }

        private void SetupDialogServiceAskUserToSaveFile(string fileName, string outputFilename)
        {
            _dialogServiceMock.Setup(x => x.AskUserToSaveFile(It.IsAny<string>(), fileName, It.IsAny<string[]>()))
                .Returns(outputFilename);
        }
    }
}
