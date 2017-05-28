using System.Globalization;
using System.Linq;
using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public class CultureAgnosticFormatter
    {
        private readonly CultureInfo _cultureForFloatingNumbers = new CultureInfo("en-US");

        private const int MAX_COORDINATE_VALUE_PRECISION = 8;
        private const double DISTANCE_IN_METERS_THRESHOLD = 2000;

        public string Format(double value, int precision)
        {
            var format = $"0.{new string('#', precision)}";
            return value.ToString(format, _cultureForFloatingNumbers);
        }

        public string FormatCoordinates(int? precision, params IHasCoordinates[] placemarks)
        {
            precision = precision ?? MAX_COORDINATE_VALUE_PRECISION;
            return string.Join(",", placemarks
                .SelectMany(x => x.Coordinates)
                .Select(x => $"{this.Format(x.Latitude, precision.Value)},{this.Format(x.Longitude, precision.Value)}")
                .Distinct());
        }

        public string FormatDistance(double distanceInMeters)
        {
            if (distanceInMeters < DISTANCE_IN_METERS_THRESHOLD)
            {
                return $"{distanceInMeters:#,##0} m";
            }

            var distanceInKm = distanceInMeters / 1000;
            return $"{distanceInKm:#,##0} km";
        }

        public double ParseDouble(string value)
        {
            return double.Parse(value, _cultureForFloatingNumbers);
        }
    }
}
