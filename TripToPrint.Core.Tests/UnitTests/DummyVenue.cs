using TripToPrint.Core.Models.Venues;

namespace TripToPrint.Core.Tests.UnitTests
{
    public class DummyVenue : VenueBase
    {
        public override VenueSource SourceType => VenueSource.Undefined;

        public override bool IsUseless()
        {
            throw new System.NotImplementedException();
        }
    }
}
