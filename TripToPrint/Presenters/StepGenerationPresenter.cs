using System;
using System.Threading.Tasks;

using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IStepGenerationPresenter : IPresenter<StepGenerationViewModel, IStepGenerationView>, IStepPresenter
    {
    }

    public class StepGenerationPresenter : IStepGenerationPresenter
    {
        private readonly IReportGenerator _reportGenerator;
        private readonly ILogStorage _logStorage;
        private readonly ILogger _logger;

        public StepGenerationPresenter(IReportGenerator reportGenerator, ILogStorage logStorage, ILogger logger)
        {
            _reportGenerator = reportGenerator;
            _logStorage = logStorage;
            _logger = logger;
        }


        public IStepGenerationView View { get; private set; }
        public virtual StepGenerationViewModel ViewModel { get; private set; }

        public void InitializePresenter(IStepGenerationView view, StepGenerationViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepGenerationViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            _logStorage.ItemAdded += (sender, item) => View.AddLogItem(item);
            _logStorage.AllItemsRemoved += (sender, args) => View.ClearLogItems();
        }

        public async Task Activated()
        {
            await GenerateReport();
        }

        public bool ValidateToGoBack()
        {
            if (ViewModel.ProgressInPercentage < 100)
            {
                // TODO: Break the process
            }

            return true;
        }

        public bool ValidateToGoNext()
        {
            if (ViewModel.ProgressInPercentage < 100)
            {
                return false;
            }

            return true;
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
        }

        private async Task GenerateReport()
        {
            _logStorage.ClearAll();
            ViewModel.ProgressInPercentage = 0;

            try
            {
                await _reportGenerator.Generate(ViewModel.InputUri, ViewModel.OutputFileName, CreateProgressTracker());
                _logger.Info("Generation process complete");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Error("Process stopped due to error");
            }
        }

        private IProgressTracker CreateProgressTracker()
        {
            return new ProgressTracker(valueInPercentage => ViewModel.ProgressInPercentage = valueInPercentage);
        }
    }
}
