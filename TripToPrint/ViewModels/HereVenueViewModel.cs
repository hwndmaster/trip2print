using TripToPrint.Core.Models.Venues;

namespace TripToPrint.ViewModels
{
    public class HereVenueViewModel : ViewModelBase
    {
        private HereVenue _venue;

        public VenueBase Venue
        {
            get => _venue;
            set => _venue = value as HereVenue;
        }
    }
}
