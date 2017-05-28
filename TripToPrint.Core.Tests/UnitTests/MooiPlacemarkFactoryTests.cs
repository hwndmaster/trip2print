using System.Collections.Generic;
using System.Device.Location;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;
using TripToPrint.Core.Models.Venues;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class MooiPlacemarkFactoryTests
    {
        private readonly Mock<IKmlCalculator> _kmlCalculatorMock = new Mock<IKmlCalculator>();
        private readonly Mock<IResourceNameProvider> _resourceNameMock = new Mock<IResourceNameProvider>();

        private Mock<MooiPlacemarkFactory> _factory;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new Mock<MooiPlacemarkFactory>(_kmlCalculatorMock.Object, _resourceNameMock.Object) {
                CallBase = true
            };

            _resourceNameMock.Setup(x => x.CreateFileNameForPlacemarkThumbnail(It.IsAny<MooiPlacemark>()))
                .Returns("thumb.jpg");
        }

        [TestMethod]
        public void When_converting_kmlplacemark_the_values_are_copied_correctly()
        {
            // Arrange
            var placemark = new KmlPlacemark {
                Name = "placemark-name",
                Description = "description-1",
                Coordinates = new [] { new GeoCoordinate(1, 2, 3) },
                IconPath = "icon-path"
            };

            // Act
            var result = _factory.Object.Create(placemark, new List<VenueBase>(), string.Empty);

            // Verify
            Assert.AreEqual(placemark.Name, result.Name);
            Assert.AreEqual(placemark.Description, result.Description);
            CollectionAssert.AreEqual(placemark.Coordinates, result.Coordinates);
            Assert.AreEqual(placemark.IconPath, result.IconPath);
        }

        [TestMethod]
        public void When_converting_kmlplacemark_the_description_is_filtered_and_images_are_reordered()
        {
            // Arrange
            var placemark = new KmlPlacemark
            {
                Description = "text<br><br><img src='1'/><br>text<img width='200' height='100' src='2'/><br>text http://sample.url/path/page?q=1&w=2 text"
            };

            // Act
            var result = _factory.Object.Create(placemark, new List<VenueBase>(), "root-folder");

            // Verify
            Assert.AreEqual("text<br>text<br>text <a href='http://sample.url/path/page?q=1&w=2'>http://sample.url/path/page?q=1&w=2</a> text", result.Description);
            Assert.AreEqual(2, result.Images.Length);
            Assert.AreEqual("1", result.Images[0]);
            Assert.AreEqual("2", result.Images[1]);
        }
    }
}
