using System;

namespace TripToPrint.ViewModels
{
    public class StepGenerationViewModel : ViewModelBase
    {
        private int _progressInPercentage;
        private string _tempPath;

        public string InputUri;
        public InputSource InputSource;

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

        public string TempPath
        {
            get { return _tempPath; }
            set
            {
                if (value == _tempPath)
                    return;
                _tempPath = value;
                OnPropertyChanged();
                TempPathChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<string> TempPathChanged;
    }
}
