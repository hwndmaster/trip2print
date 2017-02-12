using System.Collections.Generic;
using System.Device.Location;

namespace TripToPrint.Core.Models
{
    public class KmlPlacemark
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public GeoCoordinate[] Coordinates { get; set; }
        public KmlExtendedData[] ExtendedData { get; set; }
    }
}
