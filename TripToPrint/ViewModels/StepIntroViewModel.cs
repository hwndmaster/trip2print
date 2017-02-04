using System;

namespace TripToPrint.ViewModels
{
    public class StepIntroViewModel : ViewModelBase
    {
        private string _inputUri;
        private string _outputFileName;

        public string InputUri
        {
            get { return _inputUri; }
            set
            {
                if (value == _inputUri)
                    return;
                _inputUri = value;

                OnPropertyChanged();
                InputUriChanged?.Invoke(this, value);
            }
        }

        public string OutputFileName
        {
            get { return _outputFileName; }
            set
            {
                if (value == _outputFileName)
                    return;
                _outputFileName = value;

                OnPropertyChanged();
                OutputFileNameChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<string> InputUriChanged;
        public event EventHandler<string> OutputFileNameChanged;
    }
}
