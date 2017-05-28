using System;
using System.Threading.Tasks;
using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.Core.ProgressTracking;

namespace TripToPrint.Presenters
{
    public interface IStepGenerationPresenter : IStepInProgressPresenter
    {
    }

    public class StepGenerationPresenter : StepInProgressPresenterBase, IStepGenerationPresenter
    {
        private readonly IReportResourceFetcher _reportResourceFetcher;
        private readonly IProgressTrackerFactory _progressTrackerFactory;
        private readonly IUserSession _userSession;

        public StepGenerationPresenter(IReportResourceFetcher reportResourceFetcher, ILogStorage logStorage, IResourceFetchingLogger logger,
            IProgressTrackerFactory progressTrackerFactory, IUserSession userSession)
            : base(logger, logStorage)
        {
            _progressTrackerFactory = progressTrackerFactory;
            _reportResourceFetcher = reportResourceFetcher;
            _userSession = userSession;
        }

        protected override void InitializeStepSpecific()
        {
            ViewModel.StepTitle = "Wait a moment, a report is being created";
        }

        public override async Task Activated()
        {
            await base.Activated();

            await GenerateReport();
        }

        private async Task GenerateReport()
        {
            LogStorage.Clear(Logger.Category);
            ViewModel.ProgressInPercentage = 0;

            try
            {
                var progressTracker = _progressTrackerFactory.CreateForResourceFetching(value => ViewModel.ProgressInPercentage = value);

                (_userSession.GeneratedReportTempPath, _userSession.GeneratedDocument)
                    = await _reportResourceFetcher.Generate(_userSession.Document
                        , _userSession.IncludedVenues
                        , progressTracker);

                Logger.Info("Generation process complete");

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
