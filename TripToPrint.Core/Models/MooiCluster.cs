using System.Collections.Generic;
using System.Linq;

namespace TripToPrint.Core.Models
{
    public enum ClusterType
    {
        Points,
        Routes
    }

    public class MooiCluster
    {
        public MooiSection Section { get; set; }
        public List<MooiPlacemark> Placemarks { get; } = new List<MooiPlacemark>();
        public string OverviewMapFilePath { get; set; }

        public string Id => Placemarks[0].Id;
        public ClusterType Type => Placemarks?.Any(x => x.Type == PlacemarkType.Route) == true
            ? ClusterType.Routes : ClusterType.Points;
    }
}
