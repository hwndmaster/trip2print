using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IMainFormPresenter : IPresenter<MainFormViewModel, IMainFormView>, IDisposable
    {
        void AskUserToSelectKmzFile();
        Task GenerateReport();
        void OpenReport();
        void OpenReportContainingFolder();
    }

    public class MainFormPresenter : IMainFormPresenter
    {
        private readonly IDialogService _dialogService;
        private readonly IReportGenerator _reportGenerator;
        private readonly ILogStorage _logStorage;
        private readonly ILogger _logger;

        public MainFormPresenter(IDialogService dialogService, IReportGenerator reportGenerator, ILogStorage logStorage, ILogger logger)
        {
            _dialogService = dialogService;
            _reportGenerator = reportGenerator;
            _logStorage = logStorage;
            _logger = logger;
        }

        public IMainFormView View { get; private set; }
        public MainFormViewModel ViewModel { get; private set; }
        public void InitializePresenter(IMainFormView view)
        {
            ViewModel = new MainFormViewModel();
            View = view;
            View.ViewModel = ViewModel;
            View.Presenter = this;
            View.BindData();

            _logStorage.ItemAdded += (sender, item) => View.AddLogItem(item);
            _logStorage.AllItemsRemoved += (sender, args) => View.ClearLogItems();
        }

        public void Dispose()
        {
        }

        public void AskUserToSelectKmzFile()
        {
            var fileName = _dialogService.AskUserToSelectFile("Select a KMZ/KML file", filter: new [] {
                "KMZ/KML files (*.kmz, *.kml)|*.kmz;*.kml"
            });
            if (fileName == null)
                return;

            ViewModel.SelectedInputFileName = fileName;
        }

        public async Task GenerateReport()
        {
            _logStorage.ClearAll();
            ViewModel.ProgressInPercentage = 0;

            if (!ValidateInputFile())
            {
                return;
            }

            var outputFileName = Path.GetFileNameWithoutExtension(ViewModel.SelectedInputFileName);
            outputFileName = _dialogService.AskUserToSaveFile("Save output to a file", $"{outputFileName}.html", new [] {
                "HTML files (*.html)|*.html"
            });
            if (outputFileName == null)
                return;

            _logger.Info($"Output file selected: {outputFileName}");

            try
            {
                await _reportGenerator.Generate(ViewModel.SelectedInputFileName, outputFileName, CreateProgressTracker());
                ViewModel.OutputFileName = outputFileName;
                _logger.Info("Generation process complete");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Error("Process stopped due to error");
            }
        }

        public void OpenReport()
        {
            Process.Start(ViewModel.OutputFileName);
        }

        public void OpenReportContainingFolder()
        {
            string argument = "/select, \"" + ViewModel.OutputFileName + "\"";
            Process.Start("explorer.exe", argument);
        }

        private IProgressTracker CreateProgressTracker()
        {
            return new ProgressTracker(valueInPercentage => ViewModel.ProgressInPercentage = valueInPercentage);
        }

        private bool ValidateInputFile()
        {
            if (string.IsNullOrEmpty(ViewModel.SelectedInputFileName))
            {
                _logger.Warn("You have not selected an input KMZ file");
                return false;
            }
            if (!File.Exists(ViewModel.SelectedInputFileName))
            {
                _logger.Warn("The selected file was not found");
                return false;
            }
            return true;
        }
    }
}
