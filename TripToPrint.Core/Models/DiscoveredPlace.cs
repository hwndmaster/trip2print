using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripToPrint.Core.Models
{
    public class DiscoveredPlace
    {
        public string Title { get; set; }
        public GeoCoordinate Coordinate { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string Website { get; set; }
        public double AverageRating { get; set; }
        public Uri IconUrl { get; set; }
    }
}
