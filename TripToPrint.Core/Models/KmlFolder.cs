using System.Collections.Generic;
using System.Linq;

namespace TripToPrint.Core.Models
{
    public class KmlFolder
    {
        public string Name { get; set; }
        public List<KmlPlacemark> Placemarks { get; set; }

        public bool ContainsRoute => Placemarks.Any(x => x.Coordinates.Length > 1);
    }
}
