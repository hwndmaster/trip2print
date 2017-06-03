namespace TripToPrint.ReportTuning.Dto
{
    public class FoursquareVenueDto : VenueBaseDto
    {
        public double? Rating { get; set; }
        public double MaxRating { get; set; }
        public string RatingColor { get; set; }
        public int? LikesCount { get; set; }
        public string PriceLevel { get; set; }
        public string RemainingPriceLevel { get; set; }
        public string[] PhotoUrls { get; set; }
        public string[] Tags { get; set; }
        public FoursquareVenueTipDto[] Tips { get; set; }
    }
}
