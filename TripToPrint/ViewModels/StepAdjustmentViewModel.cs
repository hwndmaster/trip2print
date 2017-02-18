namespace TripToPrint.ViewModels
{
    public class StepAdjustmentViewModel : ViewModelBase
    {
        private string _outputFilePath;

        public string InputUri;
        public InputSource InputSource;
        public string TempPath;

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
    }
}
