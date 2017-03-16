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
            StepAdjustment = new StepAdjustmentViewModel();
        }

        public StepIntroViewModel StepIntro { get; private set; }
        public StepSettingViewModel StepSetting { get; private set; }
        public StepGenerationViewModel StepGeneration { get; private set; }
        public StepAdjustmentViewModel StepAdjustment { get; private set; }

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
