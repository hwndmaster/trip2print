using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TripToPrint.Core;
using TripToPrint.Core.ModelFactories;

namespace TripToPrint.Tests
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
            var content = @"<Folder><name></name>
                    <Placemark>
				        <name>placemark-1</name>
				        <description><![CDATA[description-1]]></description>
				        <Point><coordinates>1,2,3</coordinates></Point>
                    </Placemark>
                    <Placemark>
				        <name>placemark-2</name>
				        <description><![CDATA[description-2]]></description>
				        <Point><coordinates>7,8,9</coordinates></Point>
                    </Placemark>
                </Folder>";
            var xdoc = XDocument.Parse(content, LoadOptions.None);

            // Act
            var result = _factory.CreateKmlFolder(xdoc.Root);

            // Verify
            Assert.AreEqual(2, result.Placemarks.Count);
            Assert.AreEqual("placemark-1", result.Placemarks[0].Name);
            Assert.AreEqual("description-1", result.Placemarks[0].Description);
            Assert.AreEqual(2d, result.Placemarks[0].Coordinate.Latitude);
            Assert.AreEqual(1d, result.Placemarks[0].Coordinate.Longitude);
            Assert.AreEqual(3d, result.Placemarks[0].Coordinate.Altitude);
            Assert.AreEqual("placemark-2", result.Placemarks[1].Name);
            Assert.AreEqual("description-2", result.Placemarks[1].Description);
            Assert.AreEqual(8d, result.Placemarks[1].Coordinate.Latitude);
            Assert.AreEqual(7d, result.Placemarks[1].Coordinate.Longitude);
            Assert.AreEqual(9d, result.Placemarks[1].Coordinate.Altitude);
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
            Assert.AreEqual(1, result.ExtendedData.Count);
            Assert.AreEqual("media-1", result.ExtendedData[0].Name);
            Assert.AreEqual("http://2", result.ExtendedData[0].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Xml.XmlException))]
        public void When_creating_model_by_empty_content_an_exception_is_thrown()
        {
            _factory.Create(string.Empty);
        }
    }
}
