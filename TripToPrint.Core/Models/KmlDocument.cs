using System.Collections.Generic;
using System.Device.Location;

namespace TripToPrint.Core.Models
{
    public class KmlDocument
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<KmlFolder> Folders { get; set; }
    }

    public class KmlFolder
    {
        public string Name { get; set; }
        public List<KmlPlacemark> Placemarks { get; set; }
    }

    public class KmlPlacemark : IHaveCoordinate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public GeoCoordinate Coordinate { get; set; }
        public List<ExtendedData> ExtendedData { get; set; }
    }

    public class ExtendedData
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
