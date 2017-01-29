using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Xml.Linq;

using TripToPrint.Core.Models;

namespace TripToPrint.Core.ModelFactories
{
    public interface IKmlDocumentFactory
    {
        KmlDocument Create(string content);
    }

    public class KmlDocumentFactory : IKmlDocumentFactory
    {
        public KmlDocument Create(string content)
        {
            var xdoc = XDocument.Parse(content, LoadOptions.None);
            var xroot = xdoc.Root.ElementByLocalName("Document");

            var model = new KmlDocument {
                Title = xroot.ElementByLocalName("name").Value,
                Description = xroot.ElementByLocalName("description")?.Value,
                Folders = new List<KmlFolder>()
            };

            foreach (var xfolder in xroot.ElementsByLocalName("Folder"))
            {
                model.Folders.Add(CreateKmlFolder(xfolder));
            }

            return model;
        }

        public KmlFolder CreateKmlFolder(XElement xfolder)
        {
            var model = new KmlFolder {
                Name = xfolder.ElementByLocalName("name").Value,
                Placemarks = new List<KmlPlacemark>()
            };

            foreach (var xplacemark in xfolder.ElementsByLocalName("Placemark"))
            {
                var placemark = CreateKmlPlacemark(xplacemark);
                if (placemark != null)
                    model.Placemarks.Add(placemark);
            }

            return model;
        }

        public KmlPlacemark CreateKmlPlacemark(XElement xplacemark)
        {
            var model = new KmlPlacemark {
                Name = xplacemark.ElementByLocalName("name")?.Value,
                Description = xplacemark.ElementByLocalName("description")?.Value
            };

            var xstyleurl = xplacemark.ElementByLocalName("styleUrl");
            if (xstyleurl != null)
                model.IconPath = ExtractIconPath(xstyleurl);

            if (xplacemark.ElementByLocalName("Point") == null)
            {
                // TODO: This is probably a route. Now not supported
                return null;
            }

            var coordinates = xplacemark.ElementByLocalName("Point")
                .ElementByLocalName("coordinates").Value.Split(',').Select(double.Parse).ToArray();
            model.Coordinate = new GeoCoordinate(coordinates[1], coordinates[0], coordinates[2]);

            var xextendeddata = xplacemark.ElementByLocalName("ExtendedData");
            if (xextendeddata != null)
            {
                model.ExtendedData = new List<ExtendedData>();
                foreach (var xdata in xextendeddata.ElementsByLocalName("Data"))
                {
                    model.ExtendedData.Add(new ExtendedData {
                        Name = xdata.Attribute("name").Value,
                        Value = xdata.ElementByLocalName("value").Value
                    });
                }
            }

            return model;
        }

        private string ExtractIconPath(XElement xstyleurl)
        {
            var xdoc = xstyleurl.Document.Root.ElementByLocalName("Document");

            var stylemapurl = xstyleurl.Value.TrimStart('#');
            var xstylemap = xdoc.ElementsByLocalName("StyleMap").First(x => x.Attribute("id").Value == stylemapurl);
            var xpairnormal = xstylemap.ElementsByLocalName("Pair").First();
            var endstyleurl = xpairnormal.ElementByLocalName("styleUrl").Value.TrimStart('#');
            var xstyle = xdoc.ElementsByLocalName("Style").First(x => x.Attribute("id").Value == endstyleurl);
            return xstyle.ElementByLocalName("IconStyle")?.ElementByLocalName("Icon")?.ElementByLocalName("href")?.Value;
        }
    }
}
