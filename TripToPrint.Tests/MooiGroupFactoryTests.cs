using System.Collections.Generic;
using System.Device.Location;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Tests
{
    [TestClass]
    public class MooiGroupFactoryTests
    {
        private MooiGroupFactory _factory;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new MooiGroupFactory();
        }

        [TestMethod]
        public void When_creating_group_list_for_a_few_placemarks_a_single_group_is_created()
        {
            // Arrange
            KmlPlacemark[] placemarks = {
                new KmlPlacemark(),
                new KmlPlacemark(),
                new KmlPlacemark()
            };

            // Act
            var result = _factory.CreateList(placemarks);

            // Verify
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(placemarks.Length, result[0].Placemarks.Count);
        }

        [TestMethod]
        public void When_converting_kmlplacemark_the_values_are_copied_correctly()
        {
            // Arrange
            var placemark = new KmlPlacemark {
                Name = "placemark-name",
                Description = "description-1",
                Coordinate = new GeoCoordinate(1, 2, 3),
                IconPath = "icon-path"
            };

            // Act
            var result = _factory.ConvertKmlPlacemarkToMooiPlacemark(placemark);

            // Verify
            Assert.AreEqual(placemark.Name, result.Name);
            Assert.AreEqual(placemark.Description, result.Description);
            Assert.AreEqual(placemark.Coordinate, result.Coordinate);
            Assert.AreEqual(placemark.IconPath, result.IconPath);
        }

        [TestMethod]
        public void When_converting_kmlplacemark_the_description_is_filtered_and_images_are_reordered()
        {
            // Arrange
            var placemark = new KmlPlacemark
            {
                Description = "text<br><br><img 1/><br>text<img width='200' height='100' 2/><br>text"
            };

            // Act
            var result = _factory.ConvertKmlPlacemarkToMooiPlacemark(placemark);

            // Verify
            Assert.AreEqual("<img 1/><img 2/>text<br>text<br>text", result.Description);
        }
    }
}
