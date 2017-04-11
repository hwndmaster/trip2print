using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace TripToPrint.Core.Models
{
    public class KmlFolder : IKmlElement
    {
        public string Name { get; set; }
        public List<KmlPlacemark> Placemarks { get; }

        public bool ContainsRoute => Placemarks.Any(x => x.Coordinates.Length > 1);

        public KmlFolder(string name) : this(name, new KmlPlacemark[0])
        {
        }

        public KmlFolder([NotNull] IEnumerable<KmlPlacemark> placemarks) : this(null, placemarks)
        {
        }

        public KmlFolder(string name, [NotNull] IEnumerable<KmlPlacemark> placemarks)
        {
            Name = name;
            Placemarks = placemarks.ToList();
        }

        public KmlFolder CloneWithExcluding(IKmlElement[] elementsToExclude)
        {
            return new KmlFolder(Name,
                Placemarks
                    .Where(p => !elementsToExclude.Contains(p))
                    .Select(p => p.Clone()));
        }
    }
}
