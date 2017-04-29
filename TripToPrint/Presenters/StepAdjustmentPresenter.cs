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
    public interface IStepAdjustmentPresenter : IPresenter<StepAdjustmentViewModel, IStepAdjustmentView>, IStepPresenter
    {
        void OpenReport();
        void OpenReportContainingFolder();
        void CopyReportPathToClipboard();
    }

    public class StepAdjustmentPresenter : IStepAdjustmentPresenter
    {
        private readonly IDialogService _dialogService;
        private readonly IResourceNameProvider _resourceName;
        private readonly IReportGenerator _reportGenerator;
        private readonly IUserSession _userSession;
        private readonly IFileService _file;
        private readonly IAdjustBrowserViewPresenter _adjustBrowserViewPresenter;

        public StepAdjustmentPresenter(IDialogService dialogService, IResourceNameProvider resourceName,
            IReportGenerator reportGenerator, IFileService file, IUserSession userSession,
            IAdjustBrowserViewPresenter adjustBrowserViewPresenter)
        {
            _dialogService = dialogService;
            _resourceName = resourceName;
            _reportGenerator = reportGenerator;
            _file = file;
            _userSession = userSession;
            _adjustBrowserViewPresenter = adjustBrowserViewPresenter;
        }

        public virtual IStepAdjustmentView View { get; private set; }
        public virtual StepAdjustmentViewModel ViewModel { get; private set; }

        public void InitializePresenter(IStepAdjustmentView view, StepAdjustmentViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepAdjustmentViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            _adjustBrowserViewPresenter.InitializePresenter(View.AdjustBrowserView, ViewModel.AdjustBrowser);
        }

        public Task Activated()
        {
            ViewModel.AdjustBrowser.Address = Path.Combine(_userSession.GeneratedReportTempPath,
                _resourceName.GetDefaultHtmlReportName());

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


            if (!await _adjustBrowserViewPresenter.SavePdfReportAsync(outputFileName))
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
