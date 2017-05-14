using System.Collections.Generic;

using TripToPrint.Core.Models;

namespace TripToPrint
{
    public interface IUserSession
    {
        string UserLanguage { get; set; }
        string InputUri { get; set; }
        InputSource InputSource { get; set; }
        string InputFileName { get; set; }
        KmlDocument Document { get; set; }
        MooiDocument GeneratedDocument { get; set; }
        string GeneratedReportTempPath { get; set; }
        Dictionary<KmlPlacemark, DiscoveredPlace> DiscoveredPlacePerPlacemark { get; set; }
    }

    public class UserSession : IUserSession
    {
        public string UserLanguage { get; set; }
        public string InputUri { get; set; }
        public InputSource InputSource { get; set; }
        public string InputFileName { get; set; }
        public KmlDocument Document { get; set; }
        public MooiDocument GeneratedDocument { get; set; }
        public string GeneratedReportTempPath { get; set; }
        public Dictionary<KmlPlacemark, DiscoveredPlace> DiscoveredPlacePerPlacemark { get; set; }

        public UserSession()
        {
            UserLanguage = "en-US";
        }
    }
}
