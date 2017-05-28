using JetBrains.Annotations;
using TripToPrint.Core.Models.Venues;

namespace TripToPrint.Core.Models
{
    public class DiscoveredPlace
    {
        [NotNull] public VenueBase Venue { get; set; }
        [CanBeNull] public KmlPlacemark AttachedToPlacemark { get; set; }

        public bool IsForPlacemark => AttachedToPlacemark != null;

        public DiscoveredPlace Clone() => new DiscoveredPlace {
            Venue = Venue,
            AttachedToPlacemark = AttachedToPlacemark
        };
    }
}
