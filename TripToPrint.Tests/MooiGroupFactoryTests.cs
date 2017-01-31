using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Tests
{
    [TestClass]
    public class MooiGroupFactoryTests
    {
        private Mock<MooiGroupFactory> _factory;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new Mock<MooiGroupFactory> {
                CallBase = true
            };
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
            var result = _factory.Object.CreateList(placemarks);

            // Verify
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(placemarks.Length, result[0].Placemarks.Count);
        }

        [TestMethod]
        public void When_retrieving_neighbors_the_order_of_returned_placemarks_and_their_closest_neighbors_are_correct()
        {
            // Arrange
            var placemarks = new List<MooiPlacemark> {
                new MooiPlacemark { Coordinate = new GeoCoordinate(10, 10, 10) },
                new MooiPlacemark { Coordinate = new GeoCoordinate(12, 12, 12) },
                new MooiPlacemark { Coordinate = new GeoCoordinate(20, 20, 20) },
                new MooiPlacemark { Coordinate = new GeoCoordinate(22, 22, 22) },
                new MooiPlacemark { Coordinate = new GeoCoordinate(23, 23, 23) }
            };

            // Act
            var result = _factory.Object.GetPlacemarksWithNeighbors(placemarks).ToList();

            // Verify
            var expectedOrderWithNeighbors = new [] {
                new { pl = placemarks[3], neighbor = placemarks[4] },
                new { pl = placemarks[4], neighbor = placemarks[3] },
                new { pl = placemarks[2], neighbor = placemarks[3] },
                new { pl = placemarks[0], neighbor = placemarks[1] },
                new { pl = placemarks[1], neighbor = placemarks[0] }
            };
            CollectionAssert.AreEqual(expectedOrderWithNeighbors,
                result.Select(x => new { pl = x.Placemark, neighbor = x.NeighborWithMinDistance.Placemark }).ToList());
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
            var result = _factory.Object.ConvertKmlPlacemarkToMooiPlacemark(placemark);

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
            var result = _factory.Object.ConvertKmlPlacemarkToMooiPlacemark(placemark);

            // Verify
            Assert.AreEqual("<img 1/><img 2/>text<br>text<br>text", result.Description);
        }
    }
}
