using System;
using System.IO;
using System.Text.RegularExpressions;
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
    public class StepSettingPresenterTests
    {
        private readonly Mock<IStepSettingView> _viewMock = new Mock<IStepSettingView>();
        private readonly Mock<IWebClientService> _webClientkMock = new Mock<IWebClientService>();
        private readonly Mock<IFileService> _fileMock = new Mock<IFileService>();
        private readonly Mock<IGoogleMyMapAdapter> _googleMyMapAdapterMock = new Mock<IGoogleMyMapAdapter>();
        private readonly Mock<IResourceNameProvider> _resourceNameMock = new Mock<IResourceNameProvider>();
        private readonly Mock<IDialogService> _dialogMock = new Mock<IDialogService>();
        private readonly Mock<IMainWindowPresenter> _mainWindowMock = new Mock<IMainWindowPresenter>();
        private readonly Mock<IUserSession> _userSessionMock = new Mock<IUserSession>();
        private readonly Mock<IKmlFileReader> _kmlFileReaderMock = new Mock<IKmlFileReader>();

        private Mock<StepSettingPresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepSettingPresenter>(
                _googleMyMapAdapterMock.Object,
                _webClientkMock.Object,
                _fileMock.Object,
                _resourceNameMock.Object,
                _dialogMock.Object,
                _userSessionMock.Object,
                _kmlFileReaderMock.Object) {
                CallBase = true
            };

            _presenter.SetupGet(x => x.MainWindow).Returns(_mainWindowMock.Object);
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
        public async Task When_step_is_activated_with_url_provided_the_kml_is_downloaded()
        {
            // Arrange
            var kmzUrl = new Uri("http://kml-uri");
            var folderPrefix = "folder-prefix";
            // TODO: vm is needed?
            var vm = new StepSettingViewModel();
            _userSessionMock.SetupGet(x => x.InputSource).Returns(InputSource.GoogleMyMapsUrl);
            _userSessionMock.SetupGet(x => x.InputUri).Returns("http://input-uri");
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            _googleMyMapAdapterMock.Setup(x => x.GetKmlDownloadUrl(new Uri("http://input-uri"))).Returns(kmzUrl);
            _resourceNameMock.Setup(x => x.GetTempFolderPrefix()).Returns(folderPrefix);

            // Act
            await _presenter.Object.Activated();

            // Verify
            _webClientkMock.Verify(x => x.GetAsync(kmzUrl), Times.Once);
            Assert.IsTrue(Regex.IsMatch(vm.InputFileName,
                $@"{Path.GetTempPath().Replace(@"\", @"\\")}{folderPrefix}[\w\-]{{36}}\.kmz"));
        }

        [TestMethod]
        public async Task When_step_is_activated_with_invalid_url_provided_an_error_is_shown()
        {
            // Arrange
            _userSessionMock.SetupGet(x => x.InputSource).Returns(InputSource.GoogleMyMapsUrl);
            _userSessionMock.SetupGet(x => x.InputUri).Returns("http://input-uri");

            // Act
            await _presenter.Object.Activated();

            // Verify
            _dialogMock.Verify(x => x.InvalidOperationMessage(It.IsAny<string>()));
        }

        [TestMethod]
        public async Task When_step_is_activated_with_not_accessible_url_provided_an_error_is_shown()
        {
            // Arrange
            var kmzUrl = new Uri("http://kml-uri");
            _userSessionMock.SetupGet(x => x.InputSource).Returns(InputSource.GoogleMyMapsUrl);
            _userSessionMock.SetupGet(x => x.InputUri).Returns("http://input-uri");
            // TODO: vm is needed?
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepSettingViewModel());
            _googleMyMapAdapterMock.Setup(x => x.GetKmlDownloadUrl(new Uri("http://input-uri"))).Returns(kmzUrl);
            _webClientkMock.Setup(x => x.GetAsync(kmzUrl)).Throws<Exception>();

            // Act
            await _presenter.Object.Activated();

            // Verify
            _dialogMock.Verify(x => x.InvalidOperationMessage(It.IsAny<string>()));
        }
    }
}
