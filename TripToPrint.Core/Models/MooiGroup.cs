using System.Collections.Generic;
using System.Linq;

namespace TripToPrint.Core.Models
{
    public enum GroupType
    {
        Points,
        Routes
    }

    public class MooiGroup
    {
        public MooiGroup()
        {
            Placemarks = new List<MooiPlacemark>();
        }

        public MooiSection Section { get; set; }
        public List<MooiPlacemark> Placemarks { get; set; }

        public string Id => Placemarks[0].Id;
        public GroupType Type => Placemarks?.Any(x => x.Type == PlacemarkType.Route) == true
            ? GroupType.Routes : GroupType.Points;
    }
}
