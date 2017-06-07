using System;
using System.Device.Location;
using System.Linq;
using System.Xml.Linq;

using TripToPrint.Core.ExtensionMethods;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.ModelFactories
{
    public interface IKmlDocumentFactory
    {
        KmlDocument Create(string content);
    }

    internal class KmlDocumentFactory : IKmlDocumentFactory
    {
        private readonly CultureAgnosticFormatter _formatter = new CultureAgnosticFormatter();

        public KmlDocument Create(string content)
        {
            var xdoc = XDocument.Parse(content, LoadOptions.None);
            var xroot = xdoc.Root.ElementByLocalName("Document");

            var model = new KmlDocument {
                Title = xroot.ElementByLocalName("name").Value,
                Description = xroot.ElementByLocalName("description")?.Value
            };

            foreach (var xfolder in xroot.ElementsByLocalName("Folder"))
            {
                model.Folders.Add(CreateKmlFolder(xfolder));
            }

            return model;
        }

        public KmlFolder CreateKmlFolder(XElement xfolder)
        {
            var placemarks = from xplacemark in xfolder.ElementsByLocalName("Placemark")
                             let placemark = CreateKmlPlacemark(xplacemark)
                             where placemark != null
                             select placemark;

            return new KmlFolder(xfolder.ElementByLocalName("name").Value, placemarks);
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

            if (xplacemark.ElementByLocalName("Point") != null)
            {
                model.Coordinates = ReadCoordinates(xplacemark.ElementByLocalName("Point"));
            }
            else if (xplacemark.ElementByLocalName("LineString") != null)
            {
                var xlineString = xplacemark.ElementByLocalName("LineString");
                // TODO: How to apply? xlineString.ElementByLocalName("tessellate").Value;
                model.Coordinates = ReadCoordinates(xlineString);
            }

            var xextendeddata = xplacemark.ElementByLocalName("ExtendedData");
            if (xextendeddata != null)
            {
                model.ExtendedData = xextendeddata.ElementsByLocalName("Data")
                    .Select(xdata => new KmlExtendedData {
                        Name = xdata.Attribute("name")?.Value,
                        Value = xdata.ElementByLocalName("value").Value
                    })
                    .ToArray();
            }

            return model;
        }

        private GeoCoordinate[] ReadCoordinates(XElement xcontainer)
        {
            return xcontainer
                .ElementByLocalName("coordinates").Value
                .Trim('\r', '\n', ' ')
                .Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(',').Select(d => _formatter.ParseDouble(d)).ToArray())
                .Select(x => new GeoCoordinate(x[1], x[0], x[2]))
                .ToArray();
        }

        private string ExtractIconPath(XElement xstyleurl)
        {
            var xdoc = xstyleurl.Document?.Root.ElementByLocalName("Document");

            var stylemapurl = xstyleurl.Value.TrimStart('#');
            var xstylemap = xdoc.ElementsByLocalName("StyleMap").First(x => x.Attribute("id")?.Value == stylemapurl);
            var xpairnormal = xstylemap.ElementsByLocalName("Pair").First();
            var endstyleurl = xpairnormal.ElementByLocalName("styleUrl").Value.TrimStart('#');
            var xstyle = xdoc.ElementsByLocalName("Style").First(x => x.Attribute("id")?.Value == endstyleurl);
            return xstyle.ElementByLocalName("IconStyle")?.ElementByLocalName("Icon")?.ElementByLocalName("href")?.Value;
        }
    }
}
