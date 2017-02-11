using System;
using System.Device.Location;
using System.Linq;

namespace TripToPrint.Core.Models
{
    public class MooiPlacemark : IHaveCoordinate
    {
        public MooiGroup Group { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagesContent { get; set; }
        public string IconPath { get; set; }
        public GeoCoordinate Coordinate { get; set; }
        // TODO: где бы заюзать этот KmlExtendedData?
        //public List<KmlExtendedData> KmlExtendedData { get; set; }

        public string Id => string.Join("-",
            new[] { Coordinate.Latitude, Coordinate.Longitude }
                .Select(x => x.ToString("0.########")));

        public bool IconPathIsOnWeb => IconPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase);

        public override string ToString()
        {
            return $"{Id} / {Name}";
        }
    }
}
