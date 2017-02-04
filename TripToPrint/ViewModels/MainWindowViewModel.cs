namespace TripToPrint.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int _wizardStepIndex;

        public MainWindowViewModel()
        {
            StepIntro = new StepIntroViewModel();
            StepGeneration = new StepGenerationViewModel();
        }

        public StepIntroViewModel StepIntro { get; private set; }
        public StepGenerationViewModel StepGeneration { get; private set; }

        public int WizardStepIndex
        {
            get { return _wizardStepIndex; }
            set
            {
                if (value == _wizardStepIndex)
                    return;
                _wizardStepIndex = value;
                OnPropertyChanged();
            }
        }
    }
}
