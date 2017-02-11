using System;

namespace TripToPrint.ViewModels
{
    public class StepIntroViewModel : ViewModelBase
    {
        private string _inputUri;
        private InputSource _inputSource;

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

        public InputSource InputSource
        {
            get { return _inputSource; }
            set
            {
                if (value == _inputSource)
                    return;
                _inputSource = value;

                OnPropertyChanged();
                InputSourceChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<string> InputUriChanged;
        public event EventHandler<InputSource> InputSourceChanged;
    }
}
