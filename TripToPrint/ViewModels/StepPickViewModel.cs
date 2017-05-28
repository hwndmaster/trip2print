namespace TripToPrint.ViewModels
{
    public class StepPickViewModel : ViewModelBase
    {
        public string InputFileName
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }

        public KmlObjectsTreeViewModel KmlObjectsTree { get; } = new KmlObjectsTreeViewModel();
    }
}
