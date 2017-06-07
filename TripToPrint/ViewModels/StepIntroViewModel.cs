using System;

namespace TripToPrint.ViewModels
{
    public class StepIntroViewModel : ViewModelBase
    {
        public event EventHandler<string> InputUriChanged;
        public event EventHandler<InputSource> InputSourceChanged;
        public event EventHandler<string> ReportLanguageChanged;

        public string InputUri
        {
            get => GetOrDefault<string>();
            set
            {
                var newValue = value;
                if (newValue != null && newValue.Length == 0)
                {
                    newValue = null;
                }

                RaiseAndSetIfChanged(newValue, InputUriChanged);
            }
        }

        public InputSource InputSource
        {
            get => GetOrDefault<InputSource>();
            set => RaiseAndSetIfChanged(value, InputSourceChanged);
        }

        public string ReportLanguage
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value, ReportLanguageChanged);
        }
    }
}
