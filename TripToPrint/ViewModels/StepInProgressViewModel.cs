namespace TripToPrint.ViewModels
{
    public class StepInProgressViewModel : ViewModelBase
    {
        public int ProgressInPercentage
        {
            get => GetOrDefault(0);
            set => RaiseAndSetIfChanged(value);
        }

        public string StepTitle
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }
    }
}
