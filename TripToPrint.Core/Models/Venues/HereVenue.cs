using System.Linq;

namespace TripToPrint.Core.Models.Venues
{
    public class HereVenue : VenueBase
    {
        public override VenueSource SourceType => VenueSource.Here;

        public string WikipediaContent { get; set; }

        public override bool IsUseless() => string.IsNullOrEmpty(OpeningHours)
                                            && string.IsNullOrEmpty(ContactPhone)
                                            && string.IsNullOrEmpty(Address)
                                            && (Websites == null || !Websites.Any())
                                            && string.IsNullOrEmpty(WikipediaContent);
    }
}
