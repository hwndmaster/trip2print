namespace TripToPrint.ViewModels
{
    public class StepTuningViewModel : ViewModelBase
    {
        private string _outputFilePath;

        public string OutputFilePath
        {
            get { return _outputFilePath; }
            set
            {
                if (value == _outputFilePath)
                    return;
                _outputFilePath = value;
                OnPropertyChanged();
            }
        }

        public TuningBrowserViewModel TuningBrowser { get; } = new TuningBrowserViewModel();
    }
}
