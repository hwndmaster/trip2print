using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class KmlFileReaderTests
    {
        private readonly Mock<IKmlDocumentFactory> _kmlDocumentFactoryMock = new Mock<IKmlDocumentFactory>();
        private readonly Mock<IZipService> _zipServiceMock = new Mock<IZipService>();

        private Mock<KmlFileReader> _reader;

        private readonly string[] _zipFileEntries = {
            "images/resource-1",
            "images/resource-2",
            "file.kml"
        };

        [TestInitialize]
        public void TestInitialize()
        {
            _reader = new Mock<KmlFileReader>(_kmlDocumentFactoryMock.Object, _zipServiceMock.Object) {
                CallBase = true
            };
        }

        [TestMethod]
        public async Task When_reading_input_file_with_filetype_of_kmz_a_proper_handler_is_invoked()
        {
            // Arrange
            var document = new KmlDocument();
            _reader.Setup(x => x.ReadFromKmzFile("input-name.kmz"))
                .Returns(Task.FromResult(document));

            // Act
            var result = await _reader.Object.ReadFromFile("input-name.kmz");

            // Verify
            Assert.AreEqual(document, result);
        }

        [TestMethod]
        public async Task When_reading_input_file_with_filetype_of_kml_a_proper_handler_is_invoked()
        {
            // Arrange
            var document = new KmlDocument();
            _reader.Setup(x => x.ReadFromKmlFile("input-name.kml")).Returns(document);

            // Act
            var result = await _reader.Object.ReadFromFile("input-name.kml");

            // Verify
            Assert.AreEqual(document, result);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task When_reading_input_file_with_unknown_filetype_an_exception_is_thrown()
        {
            await _reader.Object.ReadFromFile("input-name.kkk");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task When_reading_kmz_file_without_kml_file_inside_an_exception_is_thrown()
        {
            // Arrange
            CreateSampleZipFile("zip-file-name", new [] { "another-file-in-zip" });

            // Act
            await _reader.Object.ReadFromKmzFile("zip-file-name");
        }

        [TestMethod]
        public async Task When_reading_kmz_file_the_kml_and_resources_are_fetched()
        {
            // Arrange
            var kmlDocument = new KmlDocument();
            var zipMock = CreateSampleZipFile("zip-file-name");
            zipMock.Setup(x => x.GetFileContent("file.kml")).Returns(Task.FromResult("kml-content"));
            _kmlDocumentFactoryMock.Setup(x => x.Create("kml-content")).Returns(kmlDocument);
            var resources = _zipFileEntries.Where(x => x.StartsWith("images/"))
                .Select((entry, i) => {
                    var resource = new KmlResource { FileName = entry, Blob = new[] { (byte)i } };
                    zipMock.Setup(x => x.GetFileBytes(entry)).Returns(resource.Blob);
                    return resource;
                })
                .ToList();

            // Act
            var result = await _reader.Object.ReadFromKmzFile("zip-file-name");

            // Verify
            Assert.AreSame(kmlDocument, result);
            CollectionAssert.AreEqual(result.Resources, resources);
        }

        private Mock<IZipFileWrapper> CreateSampleZipFile(string zipFilename, string[] entries = null)
        {
            var mock = new Mock<IZipFileWrapper>();

            mock.Setup(x => x.GetFileNames()).Returns(entries ?? _zipFileEntries);

            _zipServiceMock.Setup(x => x.Open(zipFilename)).Returns(mock.Object);

            return mock;
        }
    }
}
