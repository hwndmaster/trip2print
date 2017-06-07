using System;
using System.Device.Location;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using TripToPrint.Core.Integration;
using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class HereAdapterTests
    {
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();
        private readonly Mock<IKmlCalculator> _kmlCalculatorMock = new Mock<IKmlCalculator>();
        private readonly Mock<IWebClientService> _webClientMock = new Mock<IWebClientService>();

        private Mock<HereAdapter> _here;

        [TestInitialize]
        public void TestInitialize()
        {
            _here = new Mock<HereAdapter>(
                _loggerMock.Object
                , _kmlCalculatorMock.Object
                , _webClientMock.Object) {
                CallBase = true
            };
        }

        [TestMethod]
        public async Task When_fetching_placemark_thumbnail_the_parameters_passed_correctly()
        {
            // Arrange
            var placemark = new MooiPlacemark { Coordinates = new [] { new GeoCoordinate(1.11, 2.22) } };
            var bytesToMatch = SetupWebClient(uri => uri.AbsoluteUri.StartsWith(HereAdapter.IMAGES_MAPVIEW_URL),
                p => p.Contains("1.11,2.22") && p.Contains("z=18"));

            // Act
            var bytes = await _here.Object.FetchThumbnail(placemark);

            // Verify
            CollectionAssert.AreEqual(bytesToMatch, bytes);
        }

        [TestMethod]
        public async Task When_fetching_any_data_the_authorization_credentials_are_provided()
        {
            // Arrange
            var bytesToMatch = SetupWebClient(
                uri => uri.Query.Contains(HereAdapter.APP_ID_PARAM_NAME + "=")
                       && uri.Query.Contains(HereAdapter.APP_CODE_PARAM_NAME + "="),
                p => p.Contains("&param=value"));

            // Act
            var bytes = await _here.Object.DownloadData("http://url?", "param=value");

            // Verify
            CollectionAssert.AreEqual(bytesToMatch, bytes);
        }

        [TestMethod]
        public async Task When_fetching_overview_for_points_the_parameters_passed_correctly()
        {
            // Arrange
            var group = new MooiGroup {
                Placemarks = {
                    new MooiPlacemark { Coordinates = new [] { new GeoCoordinate(1.11, 2.22) } },
                    new MooiPlacemark { Coordinates = new [] { new GeoCoordinate(4.22, 3.11) } }
                }
            };
            var bytesToMatch = SetupWebClient(uri => uri.AbsoluteUri.StartsWith(HereAdapter.IMAGES_MAPVIEW_URL),
                p => p.Contains("1.11,2.22,4.22,3.11"));

            // Act
            var bytes = await _here.Object.FetchOverviewMap(group);

            // Verify
            CollectionAssert.AreEqual(bytesToMatch, bytes);
        }

        [TestMethod]
        public async Task When_fetching_overview_for_routes_the_parameters_passed_correctly()
        {
            // Arrange
            var group = new MooiGroup
            {
                Placemarks = {
                    new MooiPlacemark { Coordinates = new [] {
                        new GeoCoordinate(1.11, 2.22), new GeoCoordinate(4.22, 3.11)
                    } },
                    new MooiPlacemark { Coordinates = new [] {
                        new GeoCoordinate(5.66, 6.55)
                    } }
                }
            };
            var bytesToMatch = SetupWebClient(uri => uri.AbsoluteUri.StartsWith(HereAdapter.IMAGES_ROUTE_URL),
                p => p.Contains($"&{HereAdapter.IMAGES_ROUTE_ROUTE_PARAM_NAME}=1.11,2.22,4.22,3.11")
                     && p.Contains($"&{HereAdapter.IMAGES_ROUTE_POINT_PARAM_NAME}=5.66,6.55"));

            // Act
            var bytes = await _here.Object.FetchOverviewMap(group);

            // Verify
            CollectionAssert.AreEqual(bytesToMatch, bytes);
        }

        [TestMethod]
        public void Trim_out_coordinates_when_too_much_passed_within_route()
        {
            // Arrange
            var routePlacemarkToInclude = new MooiPlacemark {
                Coordinates = Enumerable.Range(1, HereAdapter.TOO_MUCH_OF_COORDINATE_POINTS * 3)
                    .Select(x => new GeoCoordinate(x * 0.001, x * 0.001)).ToArray()
            };
            var placemarkShouldBeExcluded = new MooiPlacemark {
                Coordinates = new[] { new GeoCoordinate(1, 1) }
            };
            var group = new MooiGroup {
                Placemarks = {
                    routePlacemarkToInclude,
                    placemarkShouldBeExcluded
                }
            };

            // Act
            var result = _here.Object.GetAndTrimRouteCoordinates(group);

            // Verify
            Assert.AreEqual(HereAdapter.TOO_MUCH_OF_COORDINATE_POINTS, result.Coordinates.Length);
        }

        private byte[] SetupWebClient(Expression<Func<Uri, bool>> urlMatchExpression, Expression<Func<string, bool>> paramMatchExpression)
        {
            var byteArray = new byte[] { 0x7f };
            _webClientMock
                //.Setup(x => x.PostAsync(It.Is(urlMatchExpression), It.Is(paramMatchExpression)))
                .Setup(x => x.GetAsync(It.Is<Uri>(uri =>
                    urlMatchExpression.Compile().Invoke(uri) && paramMatchExpression.Compile().Invoke(uri.Query)
                )))
                .Returns(Task.FromResult(byteArray));
            return byteArray;
        }
    }
}
