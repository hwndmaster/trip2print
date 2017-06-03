using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;
using TripToPrint.Core.Models.Venues;
using TripToPrint.Core.ProgressTracking;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class DiscoveringServiceTests
    {
        private readonly Mock<IHereAdapter> _hereAdapterMock = new Mock<IHereAdapter>();
        private readonly Mock<IFoursquareAdapter> _foursquareMock = new Mock<IFoursquareAdapter>();
        private readonly Mock<IKmlCalculator> _kmlCalculatorMock = new Mock<IKmlCalculator>();
        private readonly Mock<IDiscoveringLogger> _loggerMock = new Mock<IDiscoveringLogger>();

        private readonly Mock<IDiscoveringProgress> _progressTrackerMock = new Mock<IDiscoveringProgress>();
        private readonly Random _random = new Random();
        private readonly CancellationToken _cancel = CancellationToken.None;

        private Mock<DiscoveringService> _service;

        const string DEFAULT_LANGUAGE = "en-GB";

        [TestInitialize]
        public void TestInitialize()
        {
            _service = new Mock<DiscoveringService>(
                _hereAdapterMock.Object
                , _foursquareMock.Object
                , _kmlCalculatorMock.Object
                , _loggerMock.Object) {
                CallBase = true
            };
        }

        [TestMethod]
        public async Task Discover_processes_only_folders_without_routes()
        {
            // Arrange
            const int loopsCount = 3;
            var document = new KmlDocument {
                Folders = {
                    new KmlFolder(new [] { CreateSamplePlacemark() }),
                    new KmlFolder(new [] { CreateSamplePlacemark() }),
                    new KmlFolder(new [] { CreateSamplePlacemark(2) })
                }
            };
            var placemarksToBeProcessed = document.Folders.Take(2).SelectMany(f => f.Placemarks).ToList();
            var hereDiscovered = new List<DiscoveredPlace> {
                new DiscoveredPlace { Venue = new HereVenue { Id = "here1" } }
            };
            var foursquareDiscovered = new List<DiscoveredPlace> {
                new DiscoveredPlace { Venue = new FoursquareVenue { Id = "4sq1" } }
            };
            _service.Setup(x => x.DiscoverOnHere(placemarksToBeProcessed,
                    DEFAULT_LANGUAGE, _progressTrackerMock.Object, _cancel))
                .Returns(Task.FromResult(hereDiscovered));
            _service.Setup(x => x.DiscoverOnFoursquare(placemarksToBeProcessed,
                    DEFAULT_LANGUAGE, _progressTrackerMock.Object, _cancel))
                .Returns(Task.FromResult(foursquareDiscovered));

            // Act
            var result = await _service.Object.Discover(document, DEFAULT_LANGUAGE, _progressTrackerMock.Object, _cancel);

            // Verify
            _progressTrackerMock.Verify(x => x.ReportNumberOfIterations(placemarksToBeProcessed.Count * loopsCount));
            Assert.AreEqual(hereDiscovered[0], result[0]);
            Assert.AreEqual(foursquareDiscovered[0], result[1]);
        }

        [TestMethod]
        public async Task DiscoverOnHere_returns_correct_matching_venues()
        {
            // Arrange
            var placemarks = new List<KmlPlacemark> { CreateSamplePlacemark(), CreateSamplePlacemark() };
            _hereAdapterMock.Setup(x => x.LookupMatchingVenue(placemarks[0], DEFAULT_LANGUAGE, _cancel))
                .Returns(Task.FromResult<VenueBase>(new HereVenue { Id = "id1", Title = "venue-1" }));
            _hereAdapterMock.Setup(x => x.LookupMatchingVenue(placemarks[1], DEFAULT_LANGUAGE, _cancel))
                .Returns(Task.FromResult<VenueBase>(new HereVenue { Id = "id2", Title = "venue-2" }));

            // Act
            var result = await _service.Object.DiscoverOnHere(placemarks, DEFAULT_LANGUAGE, _progressTrackerMock.Object, _cancel);

            // Verify
            var resultOrdered = result.OrderBy(x => x.Venue.Title).ToList();
            Assert.AreEqual(placemarks.Count, result.Count);
            Assert.AreEqual("venue-1", resultOrdered[0].Venue.Title);
            Assert.AreEqual(placemarks[0], resultOrdered[0].AttachedToPlacemark);
            Assert.AreEqual("venue-2", resultOrdered[1].Venue.Title);
            Assert.AreEqual(placemarks[1], resultOrdered[1].AttachedToPlacemark);
        }

        [TestMethod]
        public async Task DiscoverOnHere_returns_no_venues_when_cancellation_token_is_checked()
        {
            // Arrange
            var cancel = new CancellationToken(true);
            var placemarks = new List<KmlPlacemark> { CreateSamplePlacemark() };
            _hereAdapterMock.Setup(x => x.LookupMatchingVenue(placemarks[0], DEFAULT_LANGUAGE, cancel))
                .Returns(Task.FromResult<VenueBase>(new HereVenue { Title = "venue" }));

            // Act
            var result = await _service.Object.DiscoverOnHere(placemarks, DEFAULT_LANGUAGE, _progressTrackerMock.Object, cancel);

            // Verify
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task When_DiscoverOnFoursquare_finds_multiple_venues_with_same_id_it_prefers_venue_for_placemarks()
        {
            // Arrange
            var venues = new[] {
                new FoursquareVenue { Id = "4sq1" },
                new FoursquareVenue { Id = "4sq2" },
                new FoursquareVenue { Id = "4sq3" }
            };
            var placemarks = new List<KmlPlacemark> {
                CreateSamplePlacemark(),
                CreateSamplePlacemark()
            };
            for (var i = 0; i < placemarks.Count; i++)
            {
                var index = i;
                _foursquareMock.Setup(x => x.LookupMatchingVenue(placemarks[index], DEFAULT_LANGUAGE, _cancel))
                    .Returns(Task.FromResult(venues[i]));
                _foursquareMock.Setup(x => x.ExplorePopular(placemarks[index], DEFAULT_LANGUAGE, _cancel))
                    .Returns(Task.FromResult(venues.ToList()));
            }

            // Act
            var result = (await _service.Object.DiscoverOnFoursquare(placemarks, DEFAULT_LANGUAGE, _progressTrackerMock.Object, _cancel))
                .OrderBy(x => x.Venue.Id)
                .ToList();

            // Verify
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(venues[0], result[0].Venue);
            Assert.AreEqual(placemarks[0], result[0].AttachedToPlacemark);
            Assert.AreEqual(venues[1], result[1].Venue);
            Assert.AreEqual(placemarks[1], result[1].AttachedToPlacemark);
            Assert.AreEqual(venues[2], result[2].Venue);
            Assert.IsNull(result[2].AttachedToPlacemark);
        }

        [TestMethod]
        public async Task DiscoverOnFoursquare_ignores_venues_with_same_name_among_placemarks()
        {
            // Arrange
            var venues = new[] {
                new FoursquareVenue { Id = "4sq1", Title = "title-1" },
                new FoursquareVenue { Id = "4sq2", Title = "title-2" },
                new FoursquareVenue { Id = "4sq3", Title = "title-3" }
            };
            var placemarks = new List<KmlPlacemark> {
                CreateSamplePlacemark(),
                CreateSamplePlacemark()
            };
            placemarks[1].Name = "title-2";
            _foursquareMock.Setup(x => x.ExplorePopular(placemarks[0], DEFAULT_LANGUAGE, _cancel))
                .Returns(Task.FromResult(venues.ToList()));

            // Act
            var result = (await _service.Object.DiscoverOnFoursquare(placemarks, DEFAULT_LANGUAGE, _progressTrackerMock.Object, _cancel))
                .OrderBy(x => x.Venue.Title).ToList();

            // Verify
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(venues[0], result[0].Venue);
            Assert.AreEqual(venues[2], result[1].Venue);
        }

        [TestMethod]
        public async Task DiscoverOnFoursquare_returns_no_venues_when_cancellation_token_is_checked()
        {
            // Arrange
            var cancel = new CancellationToken(true);
            var placemarks = new List<KmlPlacemark> { CreateSamplePlacemark() };
            _foursquareMock.Setup(x => x.LookupMatchingVenue(placemarks[0], DEFAULT_LANGUAGE, cancel))
                .Returns(Task.FromResult(new FoursquareVenue { Title = "venue", Id = "id1" }));
            _foursquareMock.Setup(x => x.ExplorePopular(placemarks[0], DEFAULT_LANGUAGE, cancel))
                .Returns(Task.FromResult(new List<FoursquareVenue> {
                    new FoursquareVenue { Title = "venue", Id = "id2" }
                }));

            // Act
            var result = await _service.Object.DiscoverOnFoursquare(placemarks, DEFAULT_LANGUAGE, _progressTrackerMock.Object, cancel);

            // Verify
            Assert.AreEqual(0, result.Count);
        }

        private KmlPlacemark CreateSamplePlacemark(int coordinatesCount = 1)
        {
            return new KmlPlacemark
            {
                Name = Guid.NewGuid().ToString(),
                Coordinates = Enumerable.Range(1, coordinatesCount)
                    .Select(x => new GeoCoordinate(_random.NextDouble() * 90, _random.NextDouble() * 180))
                    .ToArray()
            };
        }
    }
}
