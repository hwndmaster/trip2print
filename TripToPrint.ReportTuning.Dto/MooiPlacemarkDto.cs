namespace TripToPrint.ReportTuning.Dto
{
    public class MooiPlacemarkDto
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Images { get; set; }
        public string[] Coordinates { get; set; }
        public DiscoveredPlaceDto DiscoveredData { get; set; }
        public string IconPath { get; set; }
        public string ThumbnailFilePath { get; set; }
        public bool IsShape { get; set; }
        public string Distance { get; set; }
    }
}
