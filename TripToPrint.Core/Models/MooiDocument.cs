using System.Collections.Generic;
using System.Device.Location;
using System.Linq;

namespace TripToPrint.Core.Models
{
    public class MooiDocument
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<MooiSection> Sections { get; set; }
    }

    public class MooiSection
    {
        public MooiSection()
        {
            Groups = new List<MooiGroup>();
        }

        public MooiDocument Document { get; set; }
        public string Name { get; set; }
        public List<MooiGroup> Groups { get; set; }
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

        public string OverviewMapFileName => $"overview-{Id}.jpg";
    }

    public class MooiPlacemark : IHaveCoordinate
    {
        public MooiGroup Group { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagesContent { get; set; }
        public string IconPath { get; set; }
        public GeoCoordinate Coordinate { get; set; }
        // TODO: где бы заюзать этот ExtendedData?
        //public List<ExtendedData> ExtendedData { get; set; }

        public string Id => string.Join("-",
            new[] { Coordinate.Latitude, Coordinate.Longitude }
            .Select(x => x.ToString("0.########")));

        public override string ToString()
        {
            return $"{Id} / {Name}";
        }
    }
}
