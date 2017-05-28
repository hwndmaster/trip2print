using System.Threading;
using System.Threading.Tasks;

using TripToPrint.Core.Logging;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IStepInProgressPresenter : IPresenter<StepInProgressViewModel, IStepInProgressView>, IStepPresenter
    {
    }

    public abstract class StepInProgressPresenterBase : IStepInProgressPresenter
    {
        private readonly ILogger _logger;
        private readonly ILogStorage _logStorage;

        protected CancellationTokenSource CancellationTokenSource;

        private const int PROGRESS_DONE_PERCENTAGE = 100;

        protected StepInProgressPresenterBase(ILogger logger, ILogStorage logStorage)
        {
            _logger = logger;
            _logStorage = logStorage;
        }

        public IStepInProgressView View { get; private set; }
        public virtual StepInProgressViewModel ViewModel { get; private set; }
        public virtual IMainWindowPresenter MainWindow { get; set; }

        protected ILogger Logger => _logger;
        protected ILogStorage LogStorage => _logStorage;

        public void InitializePresenter(IStepInProgressView view, StepInProgressViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepInProgressViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            _logStorage.ItemAdded += (sender, item) => {
                if (item.Category == _logger.Category)
                {
                    View.AddLogItem(item);
                }
            };
            _logStorage.CategoryItemsRemoved += (sender, category) => {
                if (category == _logger.Category)
                {
                    View.ClearLogItems();
                }
            };

            InitializeStepSpecific();
        }

        protected abstract void InitializeStepSpecific();

        public virtual Task Activated()
        {
            CancellationTokenSource = new CancellationTokenSource();

            return Task.CompletedTask;
        }

        public Task<bool> BeforeGoBack()
        {
            if (ViewModel.ProgressInPercentage < PROGRESS_DONE_PERCENTAGE)
            {
                CancellationTokenSource?.Cancel();
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
    }
}
