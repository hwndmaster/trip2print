using System.Device.Location;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;
using TripToPrint.Presenters;

namespace TripToPrint
{
    [ExcludeFromCodeCoverage]
    public class TestingEnv
    {
        private readonly IUserSession _userSession;
        private readonly ILogger _logger;

        public virtual IMainWindowPresenter MainWindow { get; set; }

        public TestingEnv(IUserSession userSession, ILogger logger)
        {
            _userSession = userSession;
            _logger = logger;
        }

        public void Run()
        {
            //AdjustBrowser1();
            //FoursquareTest();
        }

        private void AdjustBrowser1()
        {
            var vm = MainWindow.ViewModel;
            vm.StepIntro.InputSource = InputSource.LocalFile;
            vm.StepIntro.InputUri = @"d:\Projects\_tasks.biz\Trip2Print\_TestingData\Италия и места.kmz";
            vm.StepPick.InputFileName = @"d:\Projects\_tasks.biz\Trip2Print\_TestingData\Италия и места.kmz";
            _userSession.GeneratedReportTempPath = @"D:\Projects\_tasks.biz\Trip2Print\_TestingData\Sample Html\";
            // _userSession.GeneratedDocument = ...
            vm.WizardStepIndex = 3;
            var wizardStep = MainWindow.GetWizardStepPresenter(vm.WizardStepIndex);
            Task.Run(async () => {
                try
                {
                    await wizardStep.Activated();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            });

        }

        private void FoursquareTest()
        {
            /*var adapter = new FoursquareAdapter(_logger, new WebClientService(_logger), new KmlCalculator());
            var pm = new KmlPlacemark {
                Name = " Restaurante El Chamo",
                Coordinates = new [] {
                    new GeoCoordinate(28.11924, -16.67092),
                }
            };

            CancellationTokenSource cancel = new CancellationTokenSource();
            Task.Run(async () => {
                try
                {
                    var result = await adapter.LookupMatchingVenue(pm, "ru", cancel.Token);
                    //var result = await adapter.ExplorePopular(pm, "en", cancel.Token);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            });*/
        }
    }
}
