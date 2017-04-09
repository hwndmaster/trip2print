namespace TripToPrint.Core.Models
{
    public class KmlResource
    {
        public string FileName { get; set; }
        public byte[] Blob { get; set; }

        protected bool Equals(KmlResource other)
        {
            return string.Equals(FileName, other.FileName) && Equals(Blob, other.Blob);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KmlResource)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FileName?.GetHashCode() ?? 0) * 397) ^ (Blob?.GetHashCode() ?? 0);
            }
        }

        public KmlResource Clone()
        {
            return new KmlResource {
                FileName = this.FileName,
                Blob = this.Blob
            };
        }
    }
}
