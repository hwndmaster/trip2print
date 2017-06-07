using System;
using System.Device.Location;
using System.Linq;
using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IKmlCalculator
    {
        double GetDistanceInMeters(IHasCoordinates placemark1, IHasCoordinates placemark2);
        double? GetDistanceInMeters(GeoCoordinate coordinate1, GeoCoordinate coordinate2);
        double CalculateRouteDistanceInMeters(IHasCoordinates placemark);
        bool CompleteFolderIsRoute(KmlFolder folder);
        bool PlacemarkIsShape(IHasCoordinates placemark);
    }

    internal class KmlCalculator : IKmlCalculator
    {
        public double GetDistanceInMeters(IHasCoordinates placemark1, IHasCoordinates placemark2)
        {
            if (placemark1.Coordinates?.Length != 1 || placemark2.Coordinates?.Length != 1)
            {
                throw new InvalidOperationException("Placemarks with multiple or zero coordinates are not supported");
            }

            return placemark1.Coordinates[0].GetDistanceTo(placemark2.Coordinates[0]);
        }

        public double? GetDistanceInMeters(GeoCoordinate coordinate1, GeoCoordinate coordinate2)
        {
            if (coordinate1 == null || coordinate2 == null)
            {
                return null;
            }
            return coordinate1.GetDistanceTo(coordinate2);
        }

        public double CalculateRouteDistanceInMeters(IHasCoordinates placemark)
        {
            if (placemark.Coordinates.Length < 2)
            {
                return 0d;
            }

            var sum = 0d;
            for (var i = 1; i < placemark.Coordinates.Length; i++)
            {
                sum += placemark.Coordinates[i].GetDistanceTo(placemark.Coordinates[i - 1]);
            }

            return sum;
        }

        public bool CompleteFolderIsRoute(KmlFolder folder)
        {
            var routes = folder.Placemarks.Where(x => x.Coordinates.Length > 1).ToList();
            if (routes.Count != 1)
            {
                return false;
            }

            Func<double, int> rounder = (d) => (int)Math.Round(d * 1000);

            var points = folder.Placemarks.Where(x => x.Coordinates.Length == 1)
                .Select(x => new[] { rounder(x.Coordinates[0].Latitude), rounder(x.Coordinates[0].Longitude) })
                .ToList();
            var routeCoords = routes[0].Coordinates.Select(x => new[] { rounder(x.Latitude), rounder(x.Longitude) }).ToList();

            var pointsOutsideRoute = points.Any(x => !routeCoords.Any(y => y[0] == x[0] && y[1] == x[1]));

            return pointsOutsideRoute != true;
        }

        public bool PlacemarkIsShape(IHasCoordinates placemark)
        {
            return placemark.Coordinates.Length > 1;
        }
    }
}
