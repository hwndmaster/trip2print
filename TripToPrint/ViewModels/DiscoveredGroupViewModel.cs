using System.Collections.ObjectModel;
using TripToPrint.Core.Models;

namespace TripToPrint.ViewModels
{
    public class DiscoveredGroupViewModel : ViewModelBase
    {
        public string Name
        {
            get => GetOrDefault<string>();
            set => RaiseAndSetIfChanged(value);
        }

        public ObservableCollection<DiscoveredVenueViewModel> Venues
            => GetOrDefault(new ObservableCollection<DiscoveredVenueViewModel>());

        public KmlPlacemark AttachedPlacemark { get; set; }
    }
}
