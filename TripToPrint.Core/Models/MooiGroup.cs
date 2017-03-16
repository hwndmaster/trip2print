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
        public MooiSection Section { get; set; }
        public List<MooiPlacemark> Placemarks { get; } = new List<MooiPlacemark>();

        public string Id => Placemarks[0].Id;
        public GroupType Type => Placemarks?.Any(x => x.Type == PlacemarkType.Route) == true
            ? GroupType.Routes : GroupType.Points;
    }
}
