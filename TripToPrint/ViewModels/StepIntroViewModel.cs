using System;

namespace TripToPrint.ViewModels
{
    public class StepIntroViewModel : ViewModelBase
    {
        private string _inputUri;
        private InputSource _inputSource;
        private string _userLanguage;

        public string InputUri
        {
            get { return _inputUri; }
            set
            {
                if (value == _inputUri)
                    return;
                _inputUri = value;

                if (_inputUri != null && _inputUri.Length == 0)
                {
                    _inputUri = null;
                }

                OnPropertyChanged();
                InputUriChanged?.Invoke(this, _inputUri);
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

        public string UserLanguage
        {
            get { return _userLanguage; }
            set
            {
                if (value == _userLanguage)
                    return;
                _userLanguage = value;

                OnPropertyChanged();
                UserLanguageChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<string> InputUriChanged;
        public event EventHandler<InputSource> InputSourceChanged;
        public event EventHandler<string> UserLanguageChanged;
    }
}
