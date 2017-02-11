using System.Collections.Generic;

namespace TripToPrint.Core.Models
{
    public class KmlFolder
    {
        public string Name { get; set; }
        public List<KmlPlacemark> Placemarks { get; set; }
    }
}
