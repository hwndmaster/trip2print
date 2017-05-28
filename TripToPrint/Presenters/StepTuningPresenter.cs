using System.Diagnostics;
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
    public interface IStepTuningPresenter : IPresenter<StepTuningViewModel, IStepTuningView>, IStepPresenter
    {
        void OpenReport();
        void OpenReportContainingFolder();
        void CopyReportPathToClipboard();
    }

    public class StepTuningPresenter : IStepTuningPresenter
    {
        private readonly IDialogService _dialogService;
        private readonly IResourceNameProvider _resourceName;
        private readonly IReportResourceFetcher _reportResourceFetcher;
        private readonly IUserSession _userSession;
        private readonly IFileService _file;
        private readonly ITuningBrowserViewPresenter _tuningBrowserViewPresenter;

        public StepTuningPresenter(IDialogService dialogService, IResourceNameProvider resourceName,
            IReportResourceFetcher reportResourceFetcher, IFileService file, IUserSession userSession,
            ITuningBrowserViewPresenter tuningBrowserViewPresenter)
        {
            _dialogService = dialogService;
            _resourceName = resourceName;
            _reportResourceFetcher = reportResourceFetcher;
            _file = file;
            _userSession = userSession;
            _tuningBrowserViewPresenter = tuningBrowserViewPresenter;
        }

        public virtual IStepTuningView View { get; private set; }
        public virtual StepTuningViewModel ViewModel { get; private set; }

        public void InitializePresenter(IStepTuningView view, StepTuningViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepTuningViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            _tuningBrowserViewPresenter.InitializePresenter(View.TuningBrowserView, ViewModel.TuningBrowser);
        }

        public Task Activated()
        {
            _tuningBrowserViewPresenter.HandleActivated(_userSession.GeneratedReportTempPath, _userSession.GeneratedDocument);

            return Task.CompletedTask;
        }

        public Task<bool> BeforeGoBack()
        {
            ViewModel.OutputFilePath = null;

            return Task.FromResult(true);
        }

        public async Task<bool> BeforeGoNext()
        {
            ViewModel.OutputFilePath = null;

            var outputFileName = GetDesiredOutputFileName();
            outputFileName = _dialogService.AskUserToSaveFile("Save output to a file",
                $"{outputFileName}.pdf", new[] { "PDF files (*.pdf)|*.pdf" });
            if (outputFileName == null)
            {
                return false;
            }


            if (!await _tuningBrowserViewPresenter.SavePdfReportAsync(outputFileName))
            {
                await _dialogService.InvalidOperationMessage("An error occurred during report create. Try to save a file to another folder or using another name (For example, with Latin symbols only).");
                return false;
            }

            ViewModel.OutputFilePath = outputFileName;

            return false;
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
            next = "Create report";
        }

        public void OpenReport()
        {
            if (ValidateReportToOpen())
            {
                Process.Start(ViewModel.OutputFilePath);
            }
        }

        public void OpenReportContainingFolder()
        {
            if (ValidateReportToOpen())
            {
                string argument = "/select, \"" + ViewModel.OutputFilePath + "\"";
                Process.Start("explorer.exe", argument);
            }
        }

        public void CopyReportPathToClipboard()
        {
            if (ValidateReportToOpen())
            {
                Clipboard.SetText(ViewModel.OutputFilePath, TextDataFormat.UnicodeText);
            }
        }

        private string GetDesiredOutputFileName()
        {
            if (_userSession.InputSource == InputSource.LocalFile)
                return Path.GetFileNameWithoutExtension(_userSession.InputUri);

            // TODO: cover with unit tests
            string fileName = _userSession.Document.Title ?? "";
            Path.GetInvalidFileNameChars().ToList().ForEach(c => fileName = fileName.Replace(c.ToString(), ""));
            if (fileName.Length > 0)
                return fileName;

            return null;
        }

        private bool ValidateReportToOpen()
        {
            if (_file.Exists(ViewModel.OutputFilePath))
            {
                return true;
            }

            ViewModel.OutputFilePath = null;
            _dialogService.InvalidOperationMessage("A report no longer exists on the local disk. Please re-create it again.");
            return false;
        }
    }
}
