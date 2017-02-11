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
    public class StepIntroPresenterTests
    {
        private readonly Mock<IStepIntroView> _viewMock = new Mock<IStepIntroView>();
        private readonly Mock<IDialogService> _dialogServiceMock = new Mock<IDialogService>();
        private readonly Mock<IFileService> _fileServiceMock = new Mock<IFileService>();
        private Mock<StepIntroPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepIntroPresenter>(_dialogServiceMock.Object, _fileServiceMock.Object) {
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
        public void When_asking_user_to_select_kmz_file_and_filename_is_selected_the_input_uri_is_set()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepIntroViewModel());
            _dialogServiceMock.Setup(x => x.AskUserToSelectFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns("filename");

            // Act
            _presenter.Object.AskUserToSelectKmzFile();

            // Verify
            Assert.AreEqual("filename", _presenter.Object.ViewModel.InputUri);
        }

        [TestMethod]
        public void When_asking_user_to_select_kmz_file_and_no_filename_is_selected_the_input_uri_is_set_to_null()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepIntroViewModel());

            // Act
            _presenter.Object.AskUserToSelectKmzFile();

            // Verify
            Assert.AreEqual(null, _presenter.Object.ViewModel.InputUri);
        }

        [TestMethod]
        public void When_input_parameters_are_ready_the_validation_to_go_next_is_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel)
                .Returns(new StepIntroViewModel { InputUri = "input-uri" });
            _fileServiceMock.Setup(x => x.Exists("input-uri")).Returns(true);
            /* TODO: _dialogServiceMock.Setup(x => x.AskUserToSaveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns("output-filename");*/

            // Act
            var result = _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(true, result);
            // TODO: _dialogServiceMock.Verify(x => x.AskUserToSaveFile(It.IsAny<string>(), "input-uri.pdf", It.IsAny<string[]>()));
        }

        [TestMethod]
        public void When_input_uri_is_not_provided_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepIntroViewModel { InputUri = null });

            // Act
            var result = _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void When_input_file_is_not_found_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepIntroViewModel { InputUri = "absent-file" });

            // Act
            var result = _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
        }
    }

}
