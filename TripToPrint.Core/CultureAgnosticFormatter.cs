using System.Globalization;

namespace TripToPrint.Core
{
    public class CultureAgnosticFormatter
    {
        private readonly CultureInfo _cultureForFloatingNumbers = new CultureInfo("en-US");

        public string Format(double value, int precision)
        {
            var format = $"0.{new string('#', precision)}";
            return value.ToString(format, _cultureForFloatingNumbers);
        }

        public double ParseDouble(string value)
        {
            return double.Parse(value, _cultureForFloatingNumbers);
        }
    }
}
