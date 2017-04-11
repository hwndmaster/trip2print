using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core;
using TripToPrint.Core.Models;
using TripToPrint.Presenters;
using TripToPrint.Properties;
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
        private readonly Mock<IKmlObjectsTreePresenter> _kmlObjectsTreePresenterMock = new Mock<IKmlObjectsTreePresenter>();

        private Mock<StepSettingPresenter> _presenter;

        private const string DEFAULT_KML_URL = "http://kml-url";

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
                _kmlFileReaderMock.Object,
                _kmlObjectsTreePresenterMock.Object) {
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
        public async Task When_step_is_activated_with_url_provided_the_kml_is_downloaded_and_readed()
        {
            // Arrange
            var folderPrefix = "folder-prefix";
            var kmlDocument = new KmlDocument();
            var vm = SetupForActivatedMethod(kmlDocument);
            _resourceNameMock.Setup(x => x.GetTempFolderPrefix()).Returns(folderPrefix);

            // Act
            await _presenter.Object.Activated();

            // Verify
            _webClientkMock.Verify(x => x.GetAsync(new Uri(DEFAULT_KML_URL)), Times.Once);
            Assert.IsTrue(Regex.IsMatch(vm.InputFileName,
                $@"{Path.GetTempPath().Replace(@"\", @"\\")}{folderPrefix}[\w\-]{{36}}\.kmz"));
            _kmlObjectsTreePresenterMock.Verify(x => x.HandleActivated(kmlDocument, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task When_step_is_activated_and_kml_downloaded_but_not_readed_an_error_is_shown_and_moved_to_previous_step()
        {
            // Arrange
            SetupForActivatedMethod(null);

            // Act
            await _presenter.Object.Activated();

            // Verify
            _dialogMock.Verify(x => x.InvalidOperationMessage(Resources.Error_FailedToLoadDocument));
            _mainWindowMock.Verify(x => x.GoBack());
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
        [ExpectedException(typeof(NotSupportedException))]
        public async Task When_step_is_activated_with_invalid_input_source_an_exception_is_thrown()
        {
            // Arrange
            _userSessionMock.SetupGet(x => x.InputSource).Returns((InputSource)(-1));

            // Act
            await _presenter.Object.Activated();
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

        [TestMethod]
        public async Task When_going_next_the_session_is_updated()
        {
            // Arrange
            var kmlDocument = new KmlDocument {
                Folders = {
                    new KmlFolder("folder-1"),
                    new KmlFolder("folder-2", new[] {
                            new KmlPlacemark { Name = "pm-1" },
                            new KmlPlacemark { Name = "pm-2" }
                        }
                    )
                }
            };
            var vm = new StepSettingViewModel {
                KmlObjectsTree = {
                    FoldersToInclude = {
                        new KmlFolderNodeViewModel(kmlDocument.Folders[0]) {
                            Enabled = false
                        },
                        new KmlFolderNodeViewModel(kmlDocument.Folders[1]) {
                            Enabled = true,
                            Children = {
                                new KmlPlacemarkNodeViewModel(kmlDocument.Folders[1].Placemarks[0], null) { Enabled = false },
                                new KmlPlacemarkNodeViewModel(kmlDocument.Folders[1].Placemarks[1], null) { Enabled = true }
                            }
                        }
                    }
                }
            };
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            _presenter.Object.SetDocument(kmlDocument);
            _userSessionMock.SetupProperty(x => x.Document);

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.IsTrue(result);
            Assert.AreEqual(kmlDocument.Title, _userSessionMock.Object.Document.Title);
            Assert.AreEqual(1, _userSessionMock.Object.Document.Folders.Count);
            Assert.AreEqual(kmlDocument.Folders[1].Name, _userSessionMock.Object.Document.Folders[0].Name);
            Assert.AreEqual(1, _userSessionMock.Object.Document.Folders[0].Placemarks.Count);
            Assert.AreEqual(kmlDocument.Folders[1].Placemarks[1].Name, _userSessionMock.Object.Document.Folders[0].Placemarks[0].Name);
        }

        [TestMethod]
        public async Task When_going_next_and_placemarks_arent_available_then_false_is_returned()
        {
            // Arrange
            var kmlDocument = new KmlDocument
            {
                Folders = {
                    new KmlFolder(new[] {
                            new KmlPlacemark { Name = "pm-1" }
                        }
                    )
                }
            };
            var vm = new StepSettingViewModel
            {
                KmlObjectsTree = {
                    FoldersToInclude = {
                        new KmlFolderNodeViewModel(kmlDocument.Folders[0]) {
                            Enabled = true,
                            Children = {
                                new KmlPlacemarkNodeViewModel(kmlDocument.Folders[0].Placemarks[0], null) { Enabled = false }
                            }
                        }
                    }
                }
            };
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            _presenter.Object.SetDocument(kmlDocument);
            _userSessionMock.SetupProperty(x => x.Document);

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.IsFalse(result);
        }

        private StepSettingViewModel SetupForActivatedMethod(KmlDocument kmlDocument)
        {
            var kmzUrl = new Uri(DEFAULT_KML_URL);
            var vm = new StepSettingViewModel();
            _userSessionMock.SetupGet(x => x.InputSource).Returns(InputSource.GoogleMyMapsUrl);
            _userSessionMock.SetupGet(x => x.InputUri).Returns("http://input-uri");
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            _googleMyMapAdapterMock.Setup(x => x.GetKmlDownloadUrl(new Uri("http://input-uri"))).Returns(kmzUrl);
            _kmlFileReaderMock.Setup(x => x.ReadFromFile(It.IsAny<string>())).Returns(Task.FromResult(kmlDocument));

            return vm;
        }
    }
}
