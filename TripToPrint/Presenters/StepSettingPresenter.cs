using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TripToPrint.Core;
using TripToPrint.Core.Models;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IStepSettingPresenter : IPresenter<StepSettingViewModel, IStepSettingView>, IStepPresenter, IDisposable
    {
    }

    public class StepSettingPresenter : IStepSettingPresenter
    {
        private readonly IGoogleMyMapAdapter _googleMyMapAdapter;
        private readonly IResourceNameProvider _resourceName;
        private readonly IWebClientService _webClient;
        private readonly IDialogService _dialog;
        private readonly IUserSession _userSession;
        private readonly IFileService _file;
        private readonly IKmlFileReader _kmlFileReader;

        private readonly List<string> _tempFilesCreated = new List<string>();
        private KmlDocument _kmlDocument;

        public StepSettingPresenter(IGoogleMyMapAdapter googleMyMapAdapter, IWebClientService webClient,
            IFileService file, IResourceNameProvider resourceName, IDialogService dialog, IUserSession userSession, IKmlFileReader kmlFileReader)
        {
            _googleMyMapAdapter = googleMyMapAdapter;
            _webClient = webClient;
            _file = file;
            _resourceName = resourceName;
            _dialog = dialog;
            _userSession = userSession;
            _kmlFileReader = kmlFileReader;
        }

        public IStepSettingView View { get; private set; }
        public virtual StepSettingViewModel ViewModel { get; private set; }
        public virtual IMainWindowPresenter MainWindow { get; set; }

        public void InitializePresenter(IStepSettingView view, StepSettingViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepSettingViewModel();

            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            ViewModel.InputFileNameChanged += (sender, inputFileName) => _userSession.InputFileName = inputFileName;
        }

        public async Task Activated()
        {
            _userSession.Document = null;

            if (!await PrepareAndReadInputFile())
            {
                await MainWindow.GoBack();
                return;
            }

            _kmlDocument = await _kmlFileReader.ReadFromFile(ViewModel.InputFileName);
            if (_kmlDocument == null)
            {
                await _dialog.InvalidOperationMessage("Failed to load document");
                await MainWindow.GoBack();
                return;
            }

            // TODO: Cover with unit test
            ReadKmlDocumentIntoViewModel(_kmlDocument);
        }

        public Task<bool> BeforeGoBack()
        {
            return Task.FromResult(true);
        }

        public Task<bool> BeforeGoNext()
        {
            UpdateSessionDocument();

            return Task.FromResult(true);
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
            next = "Generate";
        }

        public void Dispose()
        {
            foreach (var tempFile in _tempFilesCreated)
            {
                if (_file.Exists(tempFile))
                {
                    _file.Delete(tempFile);
                }
            }
        }

        private async Task<bool> PrepareAndReadInputFile()
        {
            switch (_userSession.InputSource)
            {
                case InputSource.LocalFile:
                    ViewModel.InputFileName = _userSession.InputUri;
                    return true;
                case InputSource.GoogleMyMapsUrl:
                    var errorMessage = "Cannot download KML file for provided URL. Make sure the map is shared publicly.";
                    var uri = _googleMyMapAdapter.GetKmlDownloadUrl(new Uri(_userSession.InputUri));
                    if (uri == null)
                    {
                        await _dialog.InvalidOperationMessage(errorMessage);
                    }
                    try
                    {
                        var inputData = await _webClient.GetAsync(uri);
                        ViewModel.InputFileName = $"{Path.GetTempPath()}{_resourceName.GetTempFolderPrefix()}{Guid.NewGuid()}.kmz";
                        await _file.WriteBytesAsync(ViewModel.InputFileName, inputData);

                        _tempFilesCreated.Add(ViewModel.InputFileName);
                        return true;
                    }
                    catch (Exception)
                    {
                        await _dialog.InvalidOperationMessage(errorMessage);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Input source type is invalid: {_userSession.InputSource}");
            }

            return false;
        }

        private void ReadKmlDocumentIntoViewModel(KmlDocument document)
        {
            ViewModel.FoldersToInclude.Clear();
            foreach (var folder in document.Folders)
            {
                var folderVm = new KmlFolderNodeViewModel(folder) { Enabled = true };
                foreach (var placemark in folder.Placemarks)
                {
                    folderVm.Children.Add(new KmlPlacemarkNodeViewModel(placemark) { Enabled = true });
                }
                ViewModel.FoldersToInclude.Add(folderVm);
            }
        }

        private void UpdateSessionDocument()
        {
            var elementsToExclude = ViewModel.FoldersToInclude.Where(x => !x.Enabled).Select(x => x.Element)
                .Concat(ViewModel.FoldersToInclude.SelectMany(x => x.Children).Where(x => !x.Enabled).Select(x => x.Element))
                .ToArray();

            _userSession.Document = _kmlDocument.CloneWithExcluding(elementsToExclude);
        }
    }
}
