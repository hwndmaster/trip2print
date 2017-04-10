using System.Collections.Generic;
using System.Linq;

namespace TripToPrint.Core.Models
{
    public class KmlFolder : IKmlElement
    {
        public string Name { get; set; }
        public List<KmlPlacemark> Placemarks { get; set; } = new List<KmlPlacemark>();

        public bool ContainsRoute => Placemarks.Any(x => x.Coordinates.Length > 1);

        public KmlFolder CloneWithExcluding(IKmlElement[] elementsToExclude)
        {
            return new KmlFolder {
                Name = this.Name,
                Placemarks = this.Placemarks
                    .Where(p => !elementsToExclude.Contains(p))
                    .Select(p => p.Clone()).ToList()
            };
        }
    }
}
