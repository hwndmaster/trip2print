using System.Threading.Tasks;
using TripToPrint.Core.Logging;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IMainWindowPresenter : IPresenter<MainWindowViewModel, IMainWindowView>
    {
        Task GoBack();
        Task GoNext();
        void GetBackNextTitlesForCurrentStep(ref string back, ref string next);
    }

    public class MainWindowPresenter : IMainWindowPresenter
    {
        private readonly IStepIntroPresenter _stepIntroPresenter;
        private readonly IStepSettingPresenter _stepSettingPresenter;
        private readonly IStepGenerationPresenter _stepGenerationPresenter;
        private readonly IStepAdjustmentPresenter _stepAdjustmentPresenter;

        public MainWindowPresenter(IStepIntroPresenter stepIntroPresenter, IStepSettingPresenter stepSettingPresenter,
            IStepGenerationPresenter stepGenerationPresenter, IStepAdjustmentPresenter stepAdjustmentPresenter)
        {
            _stepIntroPresenter = stepIntroPresenter;
            _stepGenerationPresenter = stepGenerationPresenter;
            _stepAdjustmentPresenter = stepAdjustmentPresenter;
            _stepSettingPresenter = stepSettingPresenter;
        }


        public IMainWindowView View { get; private set; }
        public MainWindowViewModel ViewModel { get; private set; }

        public void InitializePresenter(IMainWindowView view, MainWindowViewModel viewModel)
        {
            ViewModel = viewModel ?? new MainWindowViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            _stepIntroPresenter.InitializePresenter(View.StepIntroView, ViewModel.StepIntro);
            _stepSettingPresenter.InitializePresenter(View.StepSettingView, ViewModel.StepSetting);
            _stepGenerationPresenter.InitializePresenter(View.StepGenerationView, ViewModel.StepGeneration);
            _stepAdjustmentPresenter.InitializePresenter(View.StepAdjustmentView, ViewModel.StepAdjustment);

            GetWizardStepPresenter(ViewModel.WizardStepIndex).Activated().GetAwaiter().GetResult();
        }

        public async Task GoBack()
        {
            var currentStepPresenter = GetWizardStepPresenter(ViewModel.WizardStepIndex);
            if (await currentStepPresenter.BeforeGoBack())
            {
                ViewModel.WizardStepIndex = 0;

                await GetWizardStepPresenter(ViewModel.WizardStepIndex).Activated();
            }
        }

        public async Task GoNext()
        {
            var currentStepPresenter = GetWizardStepPresenter(ViewModel.WizardStepIndex);
            if (await currentStepPresenter.BeforeGoNext())
            {
                ViewModel.WizardStepIndex++;
                var wizardStep = GetWizardStepPresenter(ViewModel.WizardStepIndex);
                if (wizardStep == null)
                {
                    ViewModel.WizardStepIndex = 0;
                    wizardStep = GetWizardStepPresenter(ViewModel.WizardStepIndex);
                }

                await wizardStep.Activated();
            }
        }

        public void GetBackNextTitlesForCurrentStep(ref string back, ref string next)
        {
            var currentStepPresenter = GetWizardStepPresenter(ViewModel.WizardStepIndex);
            currentStepPresenter.GetBackNextTitles(ref back, ref next);
        }

        private IStepPresenter GetWizardStepPresenter(int wizardStepIndex)
        {
            switch (wizardStepIndex)
            {
                case 0:
                    return _stepIntroPresenter;
                case 1:
                    return _stepSettingPresenter;
                case 2:
                    return _stepGenerationPresenter;
                case 3:
                    return _stepAdjustmentPresenter;
                default:
                    return null;
            }
        }
    }
}
