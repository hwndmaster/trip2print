namespace TripToPrint.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int _wizardStepIndex;

        public MainWindowViewModel()
        {
            StepIntro = new StepIntroViewModel();
            StepSetting = new StepSettingViewModel();
            StepGeneration = new StepGenerationViewModel();
            StepTuning = new StepTuningViewModel();
        }

        public StepIntroViewModel StepIntro { get; private set; }
        public StepSettingViewModel StepSetting { get; private set; }
        public StepGenerationViewModel StepGeneration { get; private set; }
        public StepTuningViewModel StepTuning { get; private set; }

        public int WizardStepIndex
        {
            get { return _wizardStepIndex; }
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
