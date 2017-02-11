using System.Collections.Generic;

namespace TripToPrint.Core.Models
{
    public class MooiGroup
    {
        public MooiGroup()
        {
            Placemarks = new List<MooiPlacemark>();
        }

        public MooiSection Section { get; set; }
        public List<MooiPlacemark> Placemarks { get; set; }

        public string Id => Placemarks[0].Id;
    }
}
