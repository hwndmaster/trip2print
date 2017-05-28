using System;
using System.Device.Location;

namespace TripToPrint.Core.Models.Venues
{
    public abstract class VenueBase : IEquatable<VenueBase>
    {
        public abstract VenueSource SourceType { get; }

        public virtual string Title { get; set; }
        public string Category { get; set; }
        public GeoCoordinate Coordinate { get; set; }
        public string Region { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string[] Websites { get; set; }
        public Uri IconUrl { get; set; }
        public string OpeningHours { get; set; }

        public abstract bool IsUseless();

        public bool Equals(VenueBase other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return SourceType == other.SourceType
                && string.Equals(Title, other.Title)
                && string.Equals(Category, other.Category)
                && Equals(Coordinate, other.Coordinate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((VenueBase)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Title?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Category?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Coordinate?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)SourceType;
                return hashCode;
            }
        }

        public static bool operator ==(VenueBase left, VenueBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VenueBase left, VenueBase right)
        {
            return !Equals(left, right);
        }
    }
}
