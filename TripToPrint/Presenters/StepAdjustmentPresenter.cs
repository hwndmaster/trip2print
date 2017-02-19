using System;
using System.Diagnostics;
using System.IO;
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

        public StepAdjustmentPresenter(IDialogService dialogService, IResourceNameProvider resourceName, IReportGenerator reportGenerator)
        {
            _dialogService = dialogService;
            _resourceName = resourceName;
            _reportGenerator = reportGenerator;
        }

        public virtual IStepAdjustmentView View { get; private set; }
        public virtual StepAdjustmentViewModel ViewModel { get; private set; }
        public event EventHandler GoNextRequested;

        public void InitializePresenter(IStepAdjustmentView view, StepAdjustmentViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepAdjustmentViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;
        }

        public Task Activated()
        {
            View.SetAddress(Path.Combine(ViewModel.TempPath, _resourceName.GetDefaultHtmlReportName()));

            return Task.CompletedTask;
        }

        public bool BeforeToGoBack()
        {
            return true;
        }

        public async Task<bool> BeforeGoNext()
        {
            var outputFileName = GetDesiredOutputFileName();
            outputFileName = _dialogService.AskUserToSaveFile("Save output to a file",
                $"{outputFileName}.pdf", new[] { "PDF files (*.pdf)|*.pdf" });
            if (outputFileName == null)
            {
                return false;
            }

            await _reportGenerator.SaveHtmlReportAsPdf(ViewModel.TempPath, outputFileName);

            ViewModel.OutputFilePath = outputFileName;

            return false;
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
            next = "Create report";
        }

        public void OpenReport()
        {
            Process.Start(ViewModel.OutputFilePath);
        }

        public void OpenReportContainingFolder()
        {
            string argument = "/select, \"" + ViewModel.OutputFilePath + "\"";
            Process.Start("explorer.exe", argument);
        }

        public void CopyReportPathToClipboard()
        {
            Clipboard.SetText(ViewModel.OutputFilePath, TextDataFormat.UnicodeText);
        }

        private string GetDesiredOutputFileName()
        {
            if (ViewModel.InputSource == InputSource.LocalFile)
                return Path.GetFileNameWithoutExtension(ViewModel.InputUri);

            return null;
        }
    }
}
