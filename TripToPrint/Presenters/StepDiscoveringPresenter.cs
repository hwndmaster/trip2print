using System;
using System.Threading.Tasks;

using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.Core.ProgressTracking;

namespace TripToPrint.Presenters
{
    public interface IStepDiscoveringPresenter : IStepInProgressPresenter
    {
    }

    public class StepDiscoveringPresenter : StepInProgressPresenterBase, IStepDiscoveringPresenter
    {
        private readonly IProgressTrackerFactory _progressTrackerFactory;
        private readonly IDiscoveringService _discovering;
        private readonly IUserSession _userSession;

        public StepDiscoveringPresenter(ILogStorage logStorage, IResourceFetchingLogger logger
            , IProgressTrackerFactory progressTrackerFactory, IDiscoveringService discovering, IUserSession userSession)
            : base(logger, logStorage)
        {
            _progressTrackerFactory = progressTrackerFactory;
            _discovering = discovering;
            _userSession = userSession;
        }

        protected override void InitializeStepSpecific()
        {
            ViewModel.StepTitle = "Wait a moment, discovering recommended and popular places nearby";
        }

        public override async Task Activated()
        {
            await base.Activated();

            await Discover();
        }

        private async Task Discover()
        {
            LogStorage.Clear(Logger.Category);
            ViewModel.ProgressInPercentage = 0;

            try
            {
                var progressTracker = _progressTrackerFactory.CreateForDiscovering(value => ViewModel.ProgressInPercentage = value);

                _userSession.DiscoveredVenues = await _discovering.Discover(_userSession.Document, _userSession.ReportLanguage, progressTracker, CancellationTokenSource.Token);

                Logger.Info("Discovering process complete");

                await MainWindow.GoNext();
            }
            catch (Exception ex)
            {
#if DEBUG
                Logger.Error(ex.ToString());
#else
                Logger.Error(ex.Message);
#endif
                Logger.Error("Process stopped due to error");
            }
        }
    }
}
