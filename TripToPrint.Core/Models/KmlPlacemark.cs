using System;
using System.Device.Location;
using System.Linq;

using JetBrains.Annotations;

namespace TripToPrint.Core.Models
{
    public class KmlPlacemark : IKmlElement, IHasCoordinates, IEquatable<KmlPlacemark>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        [NotNull] public GeoCoordinate[] Coordinates { get; set; } = new GeoCoordinate[0];
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

        public bool Equals(KmlPlacemark other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(Name, other.Name)
                && string.Equals(Description, other.Description)
                && string.Equals(IconPath, other.IconPath)
                && Coordinates.SequenceEqual(other.Coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((KmlPlacemark)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (IconPath?.GetHashCode() ?? 0);
                if (Coordinates.Length > 0)
                {
                    hashCode = (hashCode * 397) ^ Coordinates[0].GetHashCode();
                }
                return hashCode;
            }
        }

        public static bool operator ==(KmlPlacemark left, KmlPlacemark right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KmlPlacemark left, KmlPlacemark right)
        {
            return !Equals(left, right);
        }
    }
}
