namespace TripToPrint.ViewModels
{
    public class StepTuningViewModel : ViewModelBase
    {
        public string OutputFilePath
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }

        public TuningBrowserViewModel TuningBrowser { get; } = new TuningBrowserViewModel();
    }
}
