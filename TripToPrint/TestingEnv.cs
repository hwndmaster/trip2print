using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TripToPrint.Presenters;

namespace TripToPrint
{
    [ExcludeFromCodeCoverage]
    public class TestingEnv
    {
        private readonly IUserSession _userSession;

        public virtual IMainWindowPresenter MainWindow { get; set; }

        public TestingEnv(IUserSession userSession)
        {
            _userSession = userSession;
        }

        public void Run()
        {
            //AdjustBrowser1();
        }

        private void AdjustBrowser1()
        {
            var vm = MainWindow.ViewModel;
            vm.StepIntro.InputSource = InputSource.LocalFile;
            vm.StepIntro.InputUri = @"d:\Projects\_tasks.biz\Trip2Print\_TestingData\Италия и места.kmz";
            vm.StepSetting.InputFileName = @"d:\Projects\_tasks.biz\Trip2Print\_TestingData\Италия и места.kmz";
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
    }
}
