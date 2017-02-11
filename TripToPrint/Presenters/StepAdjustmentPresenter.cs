using System;
using System.IO;
using System.Threading.Tasks;

using TripToPrint.Core;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IStepAdjustmentPresenter : IPresenter<StepAdjustmentViewModel, IStepAdjustmentView>, IStepPresenter
    {
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

        public IStepAdjustmentView View { get; private set; }
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

        public bool BeforeGoNext()
        {
            var outputFileName = GetDesiredOutputFileName();
            outputFileName = _dialogService.AskUserToSaveFile("Save output to a file",
                $"{outputFileName}.pdf", new[] { "PDF files (*.pdf)|*.pdf" });
            if (outputFileName == null)
            {
                return false;
            }

            _reportGenerator.SaveHtmlReportAsPdf(ViewModel.TempPath, outputFileName);

            return false;
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
            next = "Create report";
        }

        // TODO: Use this method on the last wizard step
        /*public void OpenReport()
        {
            Process.Start(ViewModel.OutputFileName);
        }*/

        // TODO: Use this method on the last wizard step
        /*public void OpenReportContainingFolder()
        {
            string argument = "/select, \"" + ViewModel.OutputFileName + "\"";
            Process.Start("explorer.exe", argument);
        }*/

        private string GetDesiredOutputFileName()
        {
            if (ViewModel.InputSource == InputSource.LocalFile)
                return Path.GetFileNameWithoutExtension(ViewModel.InputUri);

            return null;
        }
    }
}
