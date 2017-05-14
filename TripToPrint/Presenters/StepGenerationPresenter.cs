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
        private readonly IProgressTrackerFactory _progressTrackerFactory;
        private readonly IUserSession _userSession;

        private const int PROGRESS_DONE_PERCENTAGE = 100;

        public StepGenerationPresenter(IReportGenerator reportGenerator, ILogStorage logStorage, ILogger logger,
            IProgressTrackerFactory progressTrackerFactory, IUserSession userSession)
        {
            _progressTrackerFactory = progressTrackerFactory;
            _reportGenerator = reportGenerator;
            _logStorage = logStorage;
            _logger = logger;
            _userSession = userSession;
        }

        public IStepGenerationView View { get; private set; }
        public virtual StepGenerationViewModel ViewModel { get; private set; }
        public virtual IMainWindowPresenter MainWindow { get; set; }

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

        public Task<bool> BeforeGoBack()
        {
            if (ViewModel.ProgressInPercentage < PROGRESS_DONE_PERCENTAGE)
            {
                // TODO: Break the process
            }

            return Task.FromResult(true);
        }

        public Task<bool> BeforeGoNext()
        {
            if (ViewModel.ProgressInPercentage < PROGRESS_DONE_PERCENTAGE)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
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
                var progressTracker = _progressTrackerFactory.Create(value => ViewModel.ProgressInPercentage = value);

                (_userSession.GeneratedReportTempPath, _userSession.GeneratedDocument)
                    = await _reportGenerator.Generate(_userSession.Document
                        , _userSession.DiscoveredPlacePerPlacemark
                        , progressTracker);

                _logger.Info("Generation process complete");

                await MainWindow.GoNext();
            }
            catch (Exception ex)
            {
#if DEBUG
                _logger.Error(ex.ToString());
#else
                _logger.Error(ex.Message);
#endif
                _logger.Error("Process stopped due to error");
            }
        }
    }
}
