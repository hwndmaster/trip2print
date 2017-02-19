using System;
using System.Threading.Tasks;
using TripToPrint.Core;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IStepIntroPresenter : IPresenter<StepIntroViewModel, IStepIntroView>, IStepPresenter
    {
        void AskUserToSelectKmzFile();
    }

    public class StepIntroPresenter : IStepIntroPresenter
    {
        private readonly IDialogService _dialogService;
        private readonly IFileService _fileService;
        private readonly IGoogleMyMapAdapter _googleMyMapAdapter;

        public StepIntroPresenter(IDialogService dialogService, IFileService fileService, IGoogleMyMapAdapter googleMyMapAdapter)
        {
            _dialogService = dialogService;
            _fileService = fileService;
            _googleMyMapAdapter = googleMyMapAdapter;
        }


        public IStepIntroView View { get; private set; }
        public virtual StepIntroViewModel ViewModel { get; private set; }
        public event EventHandler GoNextRequested;

        public void InitializePresenter(IStepIntroView view, StepIntroViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepIntroViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;
        }

        public void AskUserToSelectKmzFile()
        {
            var fileName = _dialogService.AskUserToSelectFile("Select a KMZ/KML file", filter: new[] {
                "KMZ/KML files (*.kmz, *.kml)|*.kmz;*.kml"
            });
            if (fileName == null)
                return;

            ViewModel.InputUri = fileName;
        }

        public Task Activated()
        {
            return Task.CompletedTask;
        }

        public bool BeforeToGoBack()
        {
            return false;
        }

        public async Task<bool> BeforeGoNext()
        {
            if (string.IsNullOrEmpty(ViewModel.InputUri))
            {
                var message = ViewModel.InputSource == InputSource.LocalFile
                    ? "You have not selected an input KMZ file"
                    : "Please input a correct Google MyMaps URL";
                await _dialogService.InvalidOperationMessage(message);
                return false;
            }

            if (ViewModel.InputSource == InputSource.LocalFile
                && !_fileService.Exists(ViewModel.InputUri))
            {
                await _dialogService.InvalidOperationMessage("The selected file was not found");
                return false;
            }

            if (ViewModel.InputSource == InputSource.GoogleMyMapsUrl
                && !_googleMyMapAdapter.DoesLookLikeMyMapsUrl(ViewModel.InputUri))
            {
                await _dialogService.InvalidOperationMessage("The provided Google MyMaps URL doesn't look like a valid one. Ensure that the URL looks like this:\r\nhttps://www.google.com/maps/d/viewer?mid=xxx");
                return false;
            }

            return true;
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
            next = "Generate";
        }
    }
}
