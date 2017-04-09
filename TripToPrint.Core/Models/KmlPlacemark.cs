using System.Device.Location;

namespace TripToPrint.Core.Models
{
    public class KmlPlacemark : IKmlElement, IHaveCoordinates
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public GeoCoordinate[] Coordinates { get; set; }
        public KmlExtendedData[] ExtendedData { get; set; }

        public KmlPlacemark Clone()
        {
            return new KmlPlacemark {
                Name = this.Name,
                Description = this.Description,
                IconPath = this.IconPath,

                // Not necessary to do a deep clone for these properties:
                Coordinates = this.Coordinates,
                ExtendedData = this.ExtendedData
            };
        }
    }
}
