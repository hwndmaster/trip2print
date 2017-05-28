namespace TripToPrint.ReportTuning.Dto
{
    public abstract class VenueBaseDto
    {
        public string Category { get; set; }
        public string SourceType { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string Website { get; set; }
        public string OpeningHours { get; set; }
    }
}
