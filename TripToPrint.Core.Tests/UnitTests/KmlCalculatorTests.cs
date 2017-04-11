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
            var placemark = new Mock<IHaveCoordinates>();
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
            var placemark = new Mock<IHaveCoordinates>();
            placemark.SetupGet(x => x.Coordinates).Returns(new[] {
                new GeoCoordinate(1.0001, 1.0001)
            });

            // Act
            var result = _calculator.CalculateRouteDistanceInMeters(placemark.Object);

            // Verify
            Assert.AreEqual(0, result);
        }
    }
}
