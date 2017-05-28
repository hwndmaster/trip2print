using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using TripToPrint.Core;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IStepIntroPresenter : IPresenter<StepIntroViewModel, IStepIntroView>, IStepPresenter
    {
        void AskUserToSelectKmzFile();
        Task HandleInputUriDrop(IDataObject dataObject);
    }

    public class StepIntroPresenter : IStepIntroPresenter
    {
        private readonly IDialogService _dialog;
        private readonly IFileService _file;
        private readonly IGoogleMyMapAdapter _googleMyMap;
        private readonly IUserSession _userSession;

        public StepIntroPresenter(IDialogService dialog, IFileService file, IGoogleMyMapAdapter googleMyMap, IUserSession userSession)
        {
            _dialog = dialog;
            _file = file;
            _googleMyMap = googleMyMap;
            _userSession = userSession;
        }

        public IStepIntroView View { get; private set; }
        public virtual StepIntroViewModel ViewModel { get; private set; }

        public void InitializePresenter(IStepIntroView view, StepIntroViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepIntroViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            ViewModel.ReportLanguage = _userSession.ReportLanguage;

            ViewModel.InputSourceChanged += (sender, inputSource) => {
                _userSession.InputSource = inputSource;
                ViewModel.InputUri = null;
            };
            ViewModel.InputUriChanged += (sender, inputUri) => {
                _userSession.InputUri = inputUri;
            };
            ViewModel.ReportLanguageChanged += (sender, userLanguage) => {
                _userSession.ReportLanguage = userLanguage;
            };
        }

        public void AskUserToSelectKmzFile()
        {
            var fileName = _dialog.AskUserToSelectFile("Select a KMZ/KML file", filter: new[] {
                "KMZ/KML files (*.kmz, *.kml)|*.kmz;*.kml"
            });
            if (fileName == null)
                return;

            ViewModel.InputUri = fileName;
        }

        public async Task HandleInputUriDrop(IDataObject dataObject)
        {
            var errorMessage = "You may drop only KMZ/KML files or proper Google MyMaps URLs.";

            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                var filePath = ((string[]) dataObject.GetData(DataFormats.FileDrop))?.FirstOrDefault();

                if (_file.Exists(filePath))
                {
                    var fileExt = Path.GetExtension(filePath);
                    if (fileExt?.Equals(".kmz") == false && fileExt.Equals(".kml") == false)
                    {
                        await _dialog.InvalidOperationMessage("Cannot accept your file. " + errorMessage);
                        return;
                    }

                    ViewModel.InputSource = InputSource.LocalFile;
                    ViewModel.InputUri = filePath;
                }
            }
            else if (dataObject.GetDataPresent(DataFormats.Text))
            {
                var uri = (string)dataObject.GetData(DataFormats.Text);
                if (!_googleMyMap.DoesLookLikeMyMapsUrl(uri))
                {
                    await _dialog.InvalidOperationMessage("Cannot accept your URL. " + errorMessage);
                    return;
                }

                ViewModel.InputSource = InputSource.GoogleMyMapsUrl;
                ViewModel.InputUri = uri;
            }
        }

        public Task Activated()
        {
            return Task.CompletedTask;
        }

        public Task<bool> BeforeGoBack()
        {
            return Task.FromResult(false);
        }

        public async Task<bool> BeforeGoNext()
        {
            if (string.IsNullOrEmpty(ViewModel.InputUri))
            {
                var message = ViewModel.InputSource == InputSource.LocalFile
                    ? "You have not selected an input KMZ/KML file"
                    : "Please input a correct Google MyMaps URL";
                await _dialog.InvalidOperationMessage(message);
                return false;
            }

            if (ViewModel.InputSource == InputSource.LocalFile
                && !_file.Exists(ViewModel.InputUri))
            {
                await _dialog.InvalidOperationMessage("The selected file was not found");
                return false;
            }

            if (ViewModel.InputSource == InputSource.GoogleMyMapsUrl
                && !_googleMyMap.DoesLookLikeMyMapsUrl(ViewModel.InputUri))
            {
                await _dialog.InvalidOperationMessage("The provided Google MyMaps URL doesn't look like a valid one. Ensure that the URL looks like this:\r\nhttps://www.google.com/maps/d/viewer?mid=xxx");
                return false;
            }

            return true;
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
        }
    }
}
