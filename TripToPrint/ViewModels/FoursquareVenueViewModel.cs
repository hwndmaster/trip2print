using System.Linq;

using TripToPrint.Core;
using TripToPrint.Core.Models.Venues;

namespace TripToPrint.ViewModels
{
    public class FoursquareVenueViewModel : ViewModelBase
    {
        private FoursquareVenue _venue;

        public VenueBase Venue
        {
            get => _venue;
            set => _venue = value as FoursquareVenue;
        }

        public bool HasRating => _venue.Rating.HasValue;

        public string PriceLevel => _venue.PriceLevel == null
            ? null
            : string.Join(string.Empty, Enumerable.Range(1, _venue.PriceLevel.Value).Select(x => _venue.PriceCurrency));

        public string RemainingPriceLevel => _venue.PriceLevel == null
            ? null
            : string.Join(string.Empty, Enumerable.Range(1, _venue.PriceMaxLevel - _venue.PriceLevel.Value).Select(x => _venue.PriceCurrency));

        public string Distance => _venue.DistanceToPlacemark.HasValue
            ? new CultureAgnosticFormatter().FormatDistance(_venue.DistanceToPlacemark.Value)
            : null;
    }
}
