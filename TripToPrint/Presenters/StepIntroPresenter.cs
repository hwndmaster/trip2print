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

        public StepIntroPresenter(IDialogService dialogService, IFileService fileService)
        {
            _dialogService = dialogService;
            _fileService = fileService;
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

        public bool BeforeGoNext()
        {
            if (string.IsNullOrEmpty(ViewModel.InputUri))
            {
                _dialogService.InvalidOperationMessage("You have not selected an input KMZ file");
                return false;
            }

            if (ViewModel.InputSource == InputSource.LocalFile
                && !_fileService.Exists(ViewModel.InputUri))
            {
                _dialogService.InvalidOperationMessage("The selected file was not found");
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
