using TripToPrint.Core.Models.Venues;

namespace TripToPrint.Core.Tests.UnitTests
{
    public class DummyVenue : VenueBase
    {
        public override VenueSource SourceType { get; } = VenueSource.Undefined;

        public DummyVenue() { }

        public DummyVenue(VenueSource sourceType)
        {
            SourceType = sourceType;
        }

        public override bool IsUseless()
        {
            throw new System.NotImplementedException();
        }
    }
}
