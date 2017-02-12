using System;
using System.Device.Location;
using System.Linq;
using System.Text.RegularExpressions;

namespace TripToPrint.Core.Models
{
    public enum PlacemarkType
    {
        Point,
        Route
    }

    public class MooiPlacemark
    {
        public MooiGroup Group { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagesContent { get; set; }
        public string IconPath { get; set; }
        public GeoCoordinate[] Coordinates { get; set; }
        public GeoCoordinate PrimaryCoordinate => Coordinates.FirstOrDefault();
        // TODO: ��� �� ������� ���� KmlExtendedData?
        //public List<KmlExtendedData> KmlExtendedData { get; set; }

        public string Id => (int)Type + "-" + string.Join("-",
            new[] { PrimaryCoordinate.Latitude, PrimaryCoordinate.Longitude }
                .Select(x => x.ToString("0.########")));

        public PlacemarkType Type => Coordinates.Length > 1 ? PlacemarkType.Route : PlacemarkType.Point;

        public bool IconPathIsOnWeb => !string.IsNullOrEmpty(IconPath)
            && Regex.IsMatch(IconPath, @"https?://", RegexOptions.IgnoreCase);

        public override string ToString()
        {
            return $"{Id} / {Type}: {Name}";
        }
    }
}
