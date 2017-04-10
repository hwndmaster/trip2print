using System.Threading.Tasks;
using System.Windows;
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
        private readonly Mock<IGoogleMyMapAdapter> _googleMyMapAdapterMock = new Mock<IGoogleMyMapAdapter>();
        private readonly Mock<IUserSession> _userSessionMock = new Mock<IUserSession>();

        private Mock<StepIntroPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepIntroPresenter>(
                _dialogServiceMock.Object,
                _fileServiceMock.Object,
                _googleMyMapAdapterMock.Object,
                _userSessionMock.Object) {
                CallBase = true
            };
        }

        [TestMethod]
        public void When_initializing_presenter_the_properties_are_set_correctly()
        {
            // Arrange
            _userSessionMock.SetupGet(x => x.UserLanguage).Returns("language");

            // Act
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Verify
            Assert.AreEqual(_viewMock.Object, _presenter.Object.View);
            Assert.IsNotNull(_presenter.Object.ViewModel);
            Assert.AreEqual(_presenter.Object.ViewModel.UserLanguage, "language");
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
        public async Task When_input_parameters_are_ready_the_validation_to_go_next_is_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel)
                .Returns(new StepIntroViewModel { InputUri = "input-uri" });
            _fileServiceMock.Setup(x => x.Exists("input-uri")).Returns(true);

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task When_input_uri_is_not_provided_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepIntroViewModel { InputUri = null });

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task When_input_file_is_not_found_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepIntroViewModel { InputUri = "absent-file" });

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task When_input_uri_is_invalid_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepIntroViewModel {
                InputUri = "bad-url", InputSource = InputSource.GoogleMyMapsUrl
            });
            _googleMyMapAdapterMock.Setup(x => x.DoesLookLikeMyMapsUrl("bad-url")).Returns(false);

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
            _dialogServiceMock.Verify(x => x.InvalidOperationMessage(It.IsAny<string>()));
        }

        [TestMethod]
        public async Task When_dropping_proper_file_the_viewmodel_is_updated()
        {
            // Arrange
            var vm = new StepIntroViewModel { InputUri = "old-uri", InputSource = InputSource.GoogleMyMapsUrl };
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(x => x.GetDataPresent(DataFormats.FileDrop)).Returns(true);
            dataObjectMock.Setup(x => x.GetData(DataFormats.FileDrop)).Returns(new[] { "file-path.kmz" });
            _fileServiceMock.Setup(x => x.Exists("file-path.kmz")).Returns(true);

            // Act
            await _presenter.Object.HandleInputUriDrop(dataObjectMock.Object);

            // Verify
            Assert.AreEqual(InputSource.LocalFile, vm.InputSource);
            Assert.AreEqual("file-path.kmz", vm.InputUri);
        }

        [TestMethod]
        public async Task When_dropping_unsupported_file_an_error_message_is_shown()
        {
            // Arrange
            var vm = new StepIntroViewModel { InputSource = InputSource.GoogleMyMapsUrl, InputUri = "old-uri" };
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(x => x.GetDataPresent(DataFormats.FileDrop)).Returns(true);
            dataObjectMock.Setup(x => x.GetData(DataFormats.FileDrop)).Returns(new[] { "file-path.bad" });
            _fileServiceMock.Setup(x => x.Exists("file-path.bad")).Returns(true);

            // Act
            await _presenter.Object.HandleInputUriDrop(dataObjectMock.Object);

            // Verify
            _dialogServiceMock.Verify(x => x.InvalidOperationMessage(It.IsAny<string>()), Times.Once);
            Assert.AreEqual(InputSource.GoogleMyMapsUrl, vm.InputSource);
            Assert.AreEqual("old-uri", vm.InputUri);
        }

        [TestMethod]
        public async Task When_dropping_proper_mymaps_link_the_viewmodel_is_updated()
        {
            // Arrange
            var vm = new StepIntroViewModel { InputUri = "old-uri", InputSource = InputSource.LocalFile };
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(x => x.GetDataPresent(DataFormats.Text)).Returns(true);
            dataObjectMock.Setup(x => x.GetData(DataFormats.Text)).Returns("new-uri");
            _googleMyMapAdapterMock.Setup(x => x.DoesLookLikeMyMapsUrl("new-uri")).Returns(true);

            // Act
            await _presenter.Object.HandleInputUriDrop(dataObjectMock.Object);

            // Verify
            Assert.AreEqual(InputSource.GoogleMyMapsUrl, vm.InputSource);
            Assert.AreEqual("new-uri", vm.InputUri);
        }

        [TestMethod]
        public async Task When_dropping_inproper_mymaps_link_an_error_message_is_shown()
        {
            // Arrange
            var vm = new StepIntroViewModel { InputUri = "old-uri", InputSource = InputSource.LocalFile };
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            var dataObjectMock = new Mock<IDataObject>();
            dataObjectMock.Setup(x => x.GetDataPresent(DataFormats.Text)).Returns(true);
            dataObjectMock.Setup(x => x.GetData(DataFormats.Text)).Returns("bad-uri");
            _googleMyMapAdapterMock.Setup(x => x.DoesLookLikeMyMapsUrl("bad-uri")).Returns(false);

            // Act
            await _presenter.Object.HandleInputUriDrop(dataObjectMock.Object);

            // Verify
            Assert.AreEqual(InputSource.LocalFile, vm.InputSource);
            Assert.AreEqual("old-uri", vm.InputUri);
        }

        [TestMethod]
        public void When_input_source_has_changed_the_input_uri_is_emptied()
        {
            // Arrange
            _presenter.Object.InitializePresenter(_viewMock.Object);
            _presenter.Object.ViewModel.InputSource = InputSource.LocalFile;
            _presenter.Object.ViewModel.InputUri = "input-uri";

            // Act
            _presenter.Object.ViewModel.InputSource = InputSource.GoogleMyMapsUrl;

            // Verify
            Assert.AreEqual(null, _presenter.Object.ViewModel.InputUri);
        }

        [TestMethod]
        public void When_input_source_or_uri_has_changed_the_usersession_is_updated()
        {
            // Arrange
            _presenter.Object.InitializePresenter(_viewMock.Object);
            _presenter.Object.ViewModel.InputSource = InputSource.LocalFile;
            _presenter.Object.ViewModel.InputUri = "input-uri";

            // Act / Verify
            _presenter.Object.ViewModel.InputSource = InputSource.GoogleMyMapsUrl;
            _userSessionMock.VerifySet(x => x.InputSource = InputSource.GoogleMyMapsUrl, Times.Once);

            _presenter.Object.ViewModel.InputUri = "uri-updated";
            _userSessionMock.VerifySet(x => x.InputUri = "uri-updated", Times.Once);
        }

        [TestMethod]
        public void When_user_language_has_changed_the_usersession_is_updated()
        {
            // Arrange
            _userSessionMock.SetupGet(x => x.UserLanguage).Returns("user-language");
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Act
            _presenter.Object.ViewModel.UserLanguage = "another-language";

            // Verify
            _userSessionMock.VerifySet(x => x.UserLanguage = "another-language", Times.Once);
        }
    }
}
