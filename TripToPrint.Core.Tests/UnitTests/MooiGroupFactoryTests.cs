using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class MooiGroupFactoryTests
    {
        private readonly Mock<IKmlCalculator> _kmlCalculatorMock = new Mock<IKmlCalculator>();
        private readonly Mock<IResourceNameProvider> _resourceNameMock = new Mock<IResourceNameProvider>();
        private readonly Mock<IMooiPlacemarkFactory> _mooiPlacemarkFactoryMock = new Mock<IMooiPlacemarkFactory>();

        private Mock<MooiGroupFactory> _factory;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new Mock<MooiGroupFactory>(
                _kmlCalculatorMock.Object,
                _resourceNameMock.Object,
                _mooiPlacemarkFactoryMock.Object) {
                CallBase = true
            };

            _resourceNameMock.Setup(x => x.CreateFileNameForPlacemarkThumbnail(It.IsAny<MooiPlacemark>()))
                .Returns("thumb.jpg");
            _resourceNameMock.Setup(x => x.CreateFileNameForOverviewMap(It.IsAny<MooiGroup>()))
                .Returns("overview.jpg");
            _mooiPlacemarkFactoryMock.Setup(x => x.Create(
                It.IsAny<KmlPlacemark>(),
                It.IsAny<DiscoveredPlace>(),
                It.IsAny<string>())).Returns(() => new MooiPlacemark());
        }

        [TestMethod]
        public void When_creating_group_list_for_a_few_placemarks_a_single_group_is_created()
        {
            // Arrange
            var folder = new KmlFolder(new List<KmlPlacemark> {
                    new KmlPlacemark(),
                    new KmlPlacemark(),
                    new KmlPlacemark()
                });

            // Act
            var result = _factory.Object.CreateList(folder, null, string.Empty);

            // Verify
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(folder.Placemarks.Count, result[0].Placemarks.Count);
        }

        [TestMethod]
        public void When_creating_group_list_for_route_a_single_group_is_created()
        {
            // Arrange
            var placemarks = Enumerable.Range(0, 10)
                .Select(x => new KmlPlacemark { Coordinates = new[] { new GeoCoordinate(1, x) } })
                .Concat(new[] {
                    new KmlPlacemark {
                        Coordinates = Enumerable.Range(0, 10).Select(x => new GeoCoordinate(1, x)).ToArray()
                    }
                });
            var folder = new KmlFolder(placemarks);
            _kmlCalculatorMock.Setup(x => x.CompleteFolderIsRoute(folder)).Returns(true);

            // Act
            var result = _factory.Object.CreateList(folder, null, string.Empty);

            // Verify
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(folder.Placemarks.Count, result[0].Placemarks.Count);
        }

        [TestMethod]
        public void When_retrieving_neighbors_the_order_of_returned_placemarks_and_their_closest_neighbors_are_correct()
        {
            // Arrange
            var placemarks = new List<MooiPlacemark> {
                new MooiPlacemark { Coordinates = new[] { new GeoCoordinate(10, 10, 10) } },
                new MooiPlacemark { Coordinates = new[] { new GeoCoordinate(12, 12, 12) } },
                new MooiPlacemark { Coordinates = new[] { new GeoCoordinate(20, 20, 20) } },
                new MooiPlacemark { Coordinates = new[] { new GeoCoordinate(22, 22, 22) } },
                new MooiPlacemark { Coordinates = new[] { new GeoCoordinate(23, 23, 23) } }
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
    }
}
