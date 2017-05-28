namespace TripToPrint.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int _wizardStepIndex;

        public MainWindowViewModel()
        {
            StepIntro = new StepIntroViewModel();
            StepPick = new StepPickViewModel();
            StepDiscovering = new StepInProgressViewModel();
            StepExplore = new StepExploreViewModel();
            StepGeneration = new StepInProgressViewModel();
            StepTuning = new StepTuningViewModel();
        }

        public StepIntroViewModel StepIntro { get; }
        public StepPickViewModel StepPick { get; }
        public StepInProgressViewModel StepDiscovering { get; }
        public StepExploreViewModel StepExplore { get; }
        public StepInProgressViewModel StepGeneration { get; }
        public StepTuningViewModel StepTuning { get; }

        public int WizardStepIndex
        {
            get => _wizardStepIndex;
            set
            {
                if (value == _wizardStepIndex)
                    return;
                _wizardStepIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoBack));
            }
        }

        public bool CanGoBack => WizardStepIndex > 0;
    }
}
