using System.Collections.ObjectModel;

namespace TripToPrint.ViewModels
{
    public class DiscoveredSectionViewModel : ViewModelBase
    {
        public string Name
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }

        public ObservableCollection<DiscoveredGroupViewModel> Groups
        {
            get => GetOrDefault(new ObservableCollection<DiscoveredGroupViewModel>());
            set => RaiseAndSetIfChanged(value);
        }
    }
}
