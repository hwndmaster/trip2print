namespace TripToPrint.ReportTuning.Dto
{
    public class MooiDocumentDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public MooiSectionDto[] Sections { get; set; }
    }
}
