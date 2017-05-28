namespace TripToPrint.Core.Models.Venues
{
    public interface IHasDistanceToPlacemark
    {
        double? DistanceToPlacemark { get; }
    }

    public class FoursquareVenue : VenueBase, IHasDistanceToPlacemark
    {
        public override VenueSource SourceType => VenueSource.Foursquare;

        public double? Rating { get; set; }
        public double MaxRating { get; set; }
        public string RatingColor { get; set; }
        public int? LikesCount { get; set; }
        public int? PriceLevel { get; set; }
        public int PriceMaxLevel { get; set; }
        public string PriceCurrency { get; set; }
        public string[] PhotoUrls { get; set; }
        public string[] Tags { get; set; }
        public string[] Phrases { get; set; }
        public FoursquareVenueTip[] Tips { get; set; }
        public double? DistanceToPlacemark { get; set; }

        public override bool IsUseless() => Rating == null
                                            && PriceLevel == null
                                            && string.IsNullOrEmpty(ContactPhone)
                                            && string.IsNullOrEmpty(OpeningHours);
    }
}
