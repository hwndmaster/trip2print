using TripToPrint.Core.Models;

namespace TripToPrint
{
    public interface IUserSession
    {
        string InputUri { get; set; }
        InputSource InputSource { get; set; }
        string InputFileName { get; set; }
        KmlDocument Document { get; set; }
        string GeneratedReportTempPath { get; set; }
    }

    public class UserSession : IUserSession
    {
        public string InputUri { get; set; }
        public InputSource InputSource { get; set; }
        public string InputFileName { get; set; }
        public KmlDocument Document { get; set; }
        public string GeneratedReportTempPath { get; set; }
    }
}
