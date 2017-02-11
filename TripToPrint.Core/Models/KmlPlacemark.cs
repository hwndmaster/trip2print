using System.Collections.Generic;
using System.Device.Location;

namespace TripToPrint.Core.Models
{
    public class KmlPlacemark : IHaveCoordinate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public GeoCoordinate Coordinate { get; set; }
        public List<KmlExtendedData> ExtendedData { get; set; }
    }
}
