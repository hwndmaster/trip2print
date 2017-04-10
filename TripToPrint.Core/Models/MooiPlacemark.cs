using System;
using System.Device.Location;
using System.Linq;
using System.Text.RegularExpressions;

namespace TripToPrint.Core.Models
{
    public interface IHaveCoordinates
    {
        GeoCoordinate[] Coordinates { get; set; }
    }

    public enum PlacemarkType
    {
        Point,
        Route
    }

    public class MooiPlacemark : IHaveCoordinates
    {
        public MooiGroup Group { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DiscoveredPlace DiscoveredData { get; set; }
        public string ImagesContent { get; set; }
        public string IconPath { get; set; }
        public GeoCoordinate[] Coordinates { get; set; } = new GeoCoordinate[0];
        public GeoCoordinate PrimaryCoordinate => Coordinates.FirstOrDefault();
        // TODO: где бы заюзать этот KmlExtendedData?
        //public List<KmlExtendedData> KmlExtendedData { get; set; }

        public string Id => (int) Type + "-" + string.Join("-",
                                new[] { PrimaryCoordinate?.Latitude, PrimaryCoordinate?.Longitude }
                                    .Where(x => x != null)
                                    .Select(x => x.Value.ToString("0.########")));

        public PlacemarkType Type => Coordinates.Length > 1 ? PlacemarkType.Route : PlacemarkType.Point;

        public bool IconPathIsOnWeb => !string.IsNullOrEmpty(IconPath)
            && Regex.IsMatch(IconPath, @"https?://", RegexOptions.IgnoreCase);

        public override string ToString()
        {
            return $"{Id} / {Type}: {Name}";
        }
    }
}
