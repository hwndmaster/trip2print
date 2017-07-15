namespace TripToPrint.ViewModels
{
    public class WizardStepButtonViewModel : ViewModelBase
    {
        /// <summary>
        /// 1-based step index
        /// </summary>
        public int Index
        {
            get => GetOrDefault<int>();
            set => RaiseAndSetIfChanged(value);
        }

        public string Title
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }

        public WizardStepState State
        {
            get => GetOrDefault(WizardStepState.Upcoming);
            set => RaiseAndSetIfChanged(value);
        }

        public bool IsFirst
        {
            get => GetOrDefault<bool>();
            set => RaiseAndSetIfChanged(value);
        }

        public bool IsLast
        {
            get => GetOrDefault<bool>();
            set => RaiseAndSetIfChanged(value);
        }
    }
}
