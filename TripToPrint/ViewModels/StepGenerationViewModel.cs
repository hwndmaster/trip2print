namespace TripToPrint.ViewModels
{
    public class StepGenerationViewModel : ViewModelBase
    {
        private int _progressInPercentage;

        public int ProgressInPercentage
        {
            get { return _progressInPercentage; }
            set
            {
                if (value == _progressInPercentage)
                    return;
                _progressInPercentage = value;
                OnPropertyChanged();
            }
        }
    }
}
