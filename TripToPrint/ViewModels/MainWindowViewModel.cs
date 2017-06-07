using System;

namespace TripToPrint.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            StepIntro = new StepIntroViewModel();
            StepPick = new StepPickViewModel();
            StepDiscovering = new StepInProgressViewModel();
            StepExplore = new StepExploreViewModel();
            StepGeneration = new StepInProgressViewModel();
            StepTuning = new StepTuningViewModel();

            WizardStepIndexChanged += (sender, wizardStepIndex) => HandleWizardStepIndexChanged();
        }

        public StepIntroViewModel StepIntro { get; }
        public StepPickViewModel StepPick { get; }
        public StepInProgressViewModel StepDiscovering { get; }
        public StepExploreViewModel StepExplore { get; }
        public StepInProgressViewModel StepGeneration { get; }
        public StepTuningViewModel StepTuning { get; }

        public EventHandler<int> WizardStepIndexChanged;

        public int WizardStepIndex
        {
            get => GetOrDefault<int>();
            set => RaiseAndSetIfChanged(value, WizardStepIndexChanged);
        }

        public bool CanGoBack => WizardStepIndex > 0;

        private void HandleWizardStepIndexChanged()
        {
            OnPropertyChanged(nameof(CanGoBack));
        }
    }
}
