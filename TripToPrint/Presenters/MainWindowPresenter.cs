using System;
using System.Threading.Tasks;

using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IMainWindowPresenter : IPresenter<MainWindowViewModel, IMainWindowView>
    {
        Task GoBack();
        Task GoNext();
        void GetBackNextTitlesForCurrentStep(ref string back, ref string next);
        IStepPresenter GetWizardStepPresenter(int wizardStepIndex);
        void BrowseUrl(Uri uri);
    }

    public sealed class MainWindowPresenter : IMainWindowPresenter
    {
        private readonly IStepIntroPresenter _stepIntroPresenter;
        private readonly IStepPickPresenter _stepPickPresenter;
        private readonly IStepDiscoveringPresenter _stepDiscoveringPresenter;
        private readonly IStepExplorePresenter _stepExplorePresenter;
        private readonly IStepGenerationPresenter _stepGenerationPresenter;
        private readonly IStepTuningPresenter _stepTuningPresenter;
        private readonly IProcessService _process;

        public MainWindowPresenter(IStepIntroPresenter stepIntroPresenter, IStepPickPresenter stepPickPresenter,
            IStepDiscoveringPresenter stepDiscoveringPresenter, IStepExplorePresenter stepExplorePresenter,
            IStepGenerationPresenter stepGenerationPresenter, IStepTuningPresenter stepTuningPresenter,
            IProcessService process)
        {
            _stepIntroPresenter = stepIntroPresenter;
            _stepPickPresenter = stepPickPresenter;
            _stepDiscoveringPresenter = stepDiscoveringPresenter;
            _stepExplorePresenter = stepExplorePresenter;
            _stepGenerationPresenter = stepGenerationPresenter;
            _stepTuningPresenter = stepTuningPresenter;
            _process = process;
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
            _stepPickPresenter.InitializePresenter(View.StepPickView, ViewModel.StepPick);
            _stepDiscoveringPresenter.InitializePresenter(View.StepDiscoveringView, ViewModel.StepDiscovering);
            _stepExplorePresenter.InitializePresenter(View.StepExploreView, ViewModel.StepExplore);
            _stepGenerationPresenter.InitializePresenter(View.StepGenerationView, ViewModel.StepGeneration);
            _stepTuningPresenter.InitializePresenter(View.StepTuningView, ViewModel.StepTuning);

            GetWizardStepPresenter(ViewModel.WizardStepIndex).Activated().GetAwaiter().GetResult();
        }

        public async Task GoBack()
        {
            var currentStepPresenter = GetWizardStepPresenter(ViewModel.WizardStepIndex);
            if (await currentStepPresenter.BeforeGoBack())
            {
                ViewModel.WizardStepIndex--;

                var step = GetWizardStepPresenter(ViewModel.WizardStepIndex);
                if (step is IStepInProgressPresenter)
                {
                    ViewModel.WizardStepIndex--;
                    step = GetWizardStepPresenter(ViewModel.WizardStepIndex);
                }

                await step.Activated();
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

        public IStepPresenter GetWizardStepPresenter(int wizardStepIndex)
        {
            switch (wizardStepIndex)
            {
                case 0:
                    return _stepIntroPresenter;
                case 1:
                    return _stepPickPresenter;
                case 2:
                    return _stepDiscoveringPresenter;
                case 3:
                    return _stepExplorePresenter;
                case 4:
                    return _stepGenerationPresenter;
                case 5:
                    return _stepTuningPresenter;
                default:
                    return null;
            }
        }

        public void BrowseUrl(Uri uri)
        {
            _process.Start(uri.AbsoluteUri);
        }
    }
}
