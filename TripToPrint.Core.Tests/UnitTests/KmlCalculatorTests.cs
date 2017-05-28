using System;
using System.Device.Location;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using TripToPrint.Core.Models;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class KmlCalculatorTests
    {
        private KmlCalculator _calculator;

        [TestInitialize]
        public void TestInitialize()
        {
            _calculator = new KmlCalculator();
        }

        [TestMethod]
        public void Valid_calculation_of_placemark_distance()
        {
            // Arrange
            var placemark = new Mock<IHasCoordinates>();
            placemark.SetupGet(x => x.Coordinates).Returns(new[] {
                new GeoCoordinate(1.0001, 1.0001),
                new GeoCoordinate(1.0002, 1.0005),
                new GeoCoordinate(1.0003, 1.0009),
                new GeoCoordinate(1.0004, 1.0012)
            });

            // Act
            var result = _calculator.CalculateRouteDistanceInMeters(placemark.Object);

            // Verify
            Assert.AreEqual(127, Math.Round(result));
        }

        [TestMethod]
        public void When_calculating_distance_with_only_one_coordinate_zero_is_returned()
        {
            // Arrange
            var placemark = new Mock<IHasCoordinates>();
            placemark.SetupGet(x => x.Coordinates).Returns(new[] {
                new GeoCoordinate(1.0001, 1.0001)
            });

            // Act
            var result = _calculator.CalculateRouteDistanceInMeters(placemark.Object);

            // Verify
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Folder_with_simple_placemarks_is_not_recognized_as_complete_route()
        {
            // Arrange
            var folder = new KmlFolder(new [] {
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate() }
                }
            });

            // Act
            var result = _calculator.CompleteFolderIsRoute(folder);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Folder_with_multiple_placemarks_with_shapes_is_not_recognized_as_complete_route()
        {
            // Arrange
            var folder = new KmlFolder(new[] {
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(), new GeoCoordinate() }
                },
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(), new GeoCoordinate() }
                }
            });

            // Act
            var result = _calculator.CompleteFolderIsRoute(folder);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Folder_with_route_as_first_placemark_and_associated_placemarks_is_recognized_as_complete_route()
        {
            // Arrange
            var folder = new KmlFolder(new[] {
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(1, 1), new GeoCoordinate(2, 2), new GeoCoordinate(3, 3) }
                },
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(1, 1) }
                },
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(2, 2) }
                },
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(3, 3) }
                }
            });

            // Act
            var result = _calculator.CompleteFolderIsRoute(folder);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Folder_with_route_as_first_placemark_and_not_associated_placemarks_is_not_recognized_as_complete_route()
        {
            // Arrange
            var folder = new KmlFolder(new[] {
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(1, 1), new GeoCoordinate(3, 3) }
                },
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(1, 1) }
                },
                new KmlPlacemark {
                    Coordinates = new [] { new GeoCoordinate(2, 2) }
                }
            });

            // Act
            var result = _calculator.CompleteFolderIsRoute(folder);

            // Verify
            Assert.IsFalse(result);
        }
    }
}
