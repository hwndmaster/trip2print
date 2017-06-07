using System.Globalization;
using System.Threading;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TripToPrint.Core.ExtensionMethods;
using TripToPrint.Core.ModelFactories;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class KmlDocumentFactoryTests
    {
        private KmlDocumentFactory _factory;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new KmlDocumentFactory();
        }

        [TestMethod]
        public void When_creating_model_the_document_title_and_description_are_extracted()
        {
            // Arrange
            var content = @"<kml>
                <Document>
                    <name>document-name</name>
                    <description><![CDATA[document-description]]></description>
                </Document>
            </kml>";

            // Act
            var result = _factory.Create(content);

            // Verify
            Assert.AreEqual("document-name", result.Title);
            Assert.AreEqual("document-description", result.Description);
        }

        [TestMethod]
        public void When_creating_model_the_folders_with_names_are_extracted()
        {
            // Arrange
            var content = @"<kml><Document><name></name><description></description>
                <Folder>
                    <name>folder-1</name>
                </Folder>
                <Folder>
                    <name>folder-2</name>
                </Folder>
            </Document></kml>";

            // Act
            var result = _factory.Create(content);

            // Verify
            Assert.AreEqual(2, result.Folders.Count);
            Assert.AreEqual("folder-1", result.Folders[0].Name);
            Assert.AreEqual("folder-2", result.Folders[1].Name);
        }

        [TestMethod]
        public void When_creating_kmlfolder_the_placemarks_with_details_are_extracted()
        {
            // Arrange
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-Fr"); // To test floating numbers parsing
            var content = @"<Folder><name></name>
                    <Placemark>
				        <name>placemark-1</name>
				        <description><![CDATA[description-1]]></description>
				        <Point><coordinates>11.33,22.11,33.44</coordinates></Point>
                    </Placemark>
                    <Placemark>
				        <name>placemark-2</name>
				        <description><![CDATA[description-2]]></description>
				        <Point><coordinates>7,8,9</coordinates></Point>
                    </Placemark>
                    <Placemark>
				        <name>route-3</name>
				        <LineString>
                            <coordinates>66,55,0 77,44,0 88,77,0</coordinates>
                        </LineString>
                    </Placemark>
                </Folder>";
            var xdoc = XDocument.Parse(content, LoadOptions.None);

            // Act
            var result = _factory.CreateKmlFolder(xdoc.Root);

            // Verify
            Assert.AreEqual(3, result.Placemarks.Count);
            AssertPlacemark(result.Placemarks[0], "placemark-1", "description-1", new[] { 11.33, 22.11, 33.44 });
            AssertPlacemark(result.Placemarks[1], "placemark-2", "description-2", new[] { 7d, 8d, 9d });
            AssertPlacemark(result.Placemarks[2], "route-3", null,
                new[] { 66d, 55d, 0d }, new[] { 77d, 44d, 0d }, new[] { 88d, 77d, 0d });
        }

        [TestMethod]
        public void When_creating_kmlplacemark_the_iconpath_is_extracted()
        {
            // Arrange
            var content = @"<kml><Document>
                <Folder>
                    <Placemark>
				        <styleUrl>#icon-map-123</styleUrl>
				        <Point><coordinates>1,2,3</coordinates></Point>
                    </Placemark>
                </Folder>
		        <StyleMap id='icon-map-123'>
			        <Pair>
				        <key>normal</key>
				        <styleUrl>#icon-link-123</styleUrl>
			        </Pair>
		        </StyleMap>
		        <Style id='icon-link-123'>
			        <IconStyle>
				        <Icon>
					        <href>icon-image-path.png</href>
				        </Icon>
			        </IconStyle>
		        </Style></Document></kml>";
            var xdoc = XDocument.Parse(content, LoadOptions.None);
            var xplacemark = xdoc.Root.ElementByLocalName("Document").ElementByLocalName("Folder").ElementByLocalName("Placemark");

            // Act
            var result = _factory.CreateKmlPlacemark(xplacemark);

            // Verify
            Assert.AreEqual("icon-image-path.png", result.IconPath);
        }

        [TestMethod]
        public void When_creating_kmlplacemark_the_extended_data_are_extracted()
        {
            // Arrange
            var content = @"<Placemark>
				    <Point><coordinates>1,2,3</coordinates></Point>
				    <ExtendedData>
					    <Data name='media-1'>
						    <value>http://2</value>
					    </Data>
				    </ExtendedData>
                </Placemark>";
            var xdoc = XDocument.Parse(content, LoadOptions.None);

            // Act
            var result = _factory.CreateKmlPlacemark(xdoc.Root);

            // Verify
            Assert.AreEqual(1, result.ExtendedData.Length);
            Assert.AreEqual("media-1", result.ExtendedData[0].Name);
            Assert.AreEqual("http://2", result.ExtendedData[0].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Xml.XmlException))]
        public void When_creating_model_by_empty_content_an_exception_is_thrown()
        {
            _factory.Create(string.Empty);
        }

        private static void AssertPlacemark(Models.KmlPlacemark placemark, string name, string description, params double[][] coords)
        {
            Assert.AreEqual(name, placemark.Name);
            Assert.AreEqual(description, placemark.Description);
            Assert.AreEqual(coords.Length, placemark.Coordinates.Length);
            for (var i = 0; i < coords.Length; i++)
            {
                Assert.AreEqual(coords[i][1], placemark.Coordinates[i].Latitude);
                Assert.AreEqual(coords[i][0], placemark.Coordinates[i].Longitude);
                Assert.AreEqual(coords[i][2], placemark.Coordinates[i].Altitude);
            }
        }
    }
}
