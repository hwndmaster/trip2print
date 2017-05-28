using TripToPrint.Core.Models.Venues;

namespace TripToPrint.ViewModels
{
    public class DiscoveredVenueViewModel : ViewModelBase
    {
        public DiscoveredVenueViewModel(VenueBase venue)
        {
            this.Venue = venue;
        }

        public bool Enabled
        {
            get => GetOrDefault(false);
            set => RaiseAndSetIfChanged(value);
        }

        public VenueBase Venue
        {
            get => GetOrDefault<VenueBase>();
            set => RaiseAndSetIfChanged(value);
        }
    }
}
