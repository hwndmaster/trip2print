using System.Device.Location;

namespace TripToPrint.Core.Models
{
    public interface IHaveCoordinate
    {
        GeoCoordinate Coordinate { get; }
    }
}
