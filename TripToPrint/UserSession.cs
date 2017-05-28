using System.Collections.Generic;
using TripToPrint.Core.Models;

namespace TripToPrint
{
    public interface IUserSession
    {
        string ReportLanguage { get; set; }
        string InputUri { get; set; }
        InputSource InputSource { get; set; }
        KmlDocument Document { get; set; }
        MooiDocument GeneratedDocument { get; set; }
        string GeneratedReportTempPath { get; set; }
        List<DiscoveredPlace> DiscoveredVenues { get; set; }
        List<DiscoveredPlace> IncludedVenues { get; }
    }

    public class UserSession : IUserSession
    {
        public string ReportLanguage { get; set; }
        public string InputUri { get; set; }
        public InputSource InputSource { get; set; }
        public KmlDocument Document { get; set; }
        public MooiDocument GeneratedDocument { get; set; }
        public string GeneratedReportTempPath { get; set; }
        public List<DiscoveredPlace> DiscoveredVenues { get; set; }
        public List<DiscoveredPlace> IncludedVenues { get; } = new List<DiscoveredPlace>();

        public UserSession()
        {
            ReportLanguage = "en-US";
        }
    }
}
