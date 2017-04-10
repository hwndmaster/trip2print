using System;
using System.Device.Location;

namespace TripToPrint.Core.Models
{
    public class DiscoveredPlace
    {
        public virtual string Title { get; set; }
        public GeoCoordinate Coordinate { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string Website { get; set; }
        public double AverageRating { get; set; }
        public Uri IconUrl { get; set; }
        public string OpeningHours { get; set; }
        public string WikipediaContent { get; set; }

        public bool IsUseless()
        {
            return string.IsNullOrEmpty(OpeningHours)
                && string.IsNullOrEmpty(ContactPhone)
                && string.IsNullOrEmpty(Address)
                && string.IsNullOrEmpty(Website)
                && string.IsNullOrEmpty(WikipediaContent);
        }
    }
}
