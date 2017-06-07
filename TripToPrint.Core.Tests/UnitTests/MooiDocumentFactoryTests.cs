using System;
using System.Collections.Generic;
using System.Device.Location;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;
using TripToPrint.Core.Properties;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class MooiDocumentFactoryTests
    {
        private readonly Mock<IMooiGroupFactory> _mooiGroupFactoryMock = new Mock<IMooiGroupFactory>();

        private MooiDocumentFactory _factory;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new MooiDocumentFactory(_mooiGroupFactoryMock.Object);
        }

        [TestMethod]
        public void When_creating_model_the_values_are_copied_correctly()
        {
            // Arrange
            var kmlDocument = new KmlDocument {
                Title = "kml-title",
                Description = "kml-desc",
                Folders = {
                    new KmlFolder("folder-1", new List<KmlPlacemark> {
                            new KmlPlacemark()
                        })
                }
            };
            _mooiGroupFactoryMock.Setup(x => x.CreateList(kmlDocument.Folders[0], null, string.Empty)).Returns(new List<MooiGroup>());

            // Act
            var result = _factory.Create(kmlDocument, null, string.Empty);

            // Verify
            Assert.AreEqual(kmlDocument.Title, result.Title);
            Assert.AreEqual(kmlDocument.Description, result.Description);
            Assert.AreEqual(kmlDocument.Folders[0].Name, result.Sections[0].Name);
        }

        [TestMethod]
        public void When_creating_model_the_discovered_places_are_appended_correctly()
        {
            // Arrange
            var kmlDocument = new KmlDocument {
                Folders = {
                    new KmlFolder(new List<KmlPlacemark> { new KmlPlacemark() })
                }
            };
            var venue = new DummyVenue { Title = "venue", IconUrl = new Uri("http://url"), Coordinate = new GeoCoordinate(1, 2) };
            var discoveredPlaces = new List<DiscoveredPlace> {
                new DiscoveredPlace { Venue = new DummyVenue(), AttachedToPlacemark = kmlDocument.Folders[0].Placemarks[0] },
                new DiscoveredPlace { Venue = venue }
            };
            var placemarkExplored = new MooiPlacemark {
                Name = venue.Title,
                IconPath = venue.IconUrl.ToString(),
                Coordinates = new []{ venue.Coordinate }
            };
            _mooiGroupFactoryMock.Setup(x => x.CreateList(kmlDocument.Folders[0], discoveredPlaces, string.Empty))
                .Returns(new List<MooiGroup>());
            _mooiGroupFactoryMock.Setup(x => x.CreateList(It.Is<KmlFolder>(f => f.Name == Resources.Kml_Folder_Explored),
                discoveredPlaces, string.Empty))
                .Returns((KmlFolder folder, List<DiscoveredPlace> dps, string path) => new List<MooiGroup> {
                    new MooiGroup {
                        Placemarks = {
                            placemarkExplored
                        }
                    }
                });

            // Act
            var result = _factory.Create(kmlDocument, discoveredPlaces, string.Empty);

            // Verify
            var sectionWithExploredPlacemarks = result.Sections[1];
            Assert.AreEqual(kmlDocument.Folders[0].Name, result.Sections[0].Name);
            Assert.AreEqual(Resources.Kml_Folder_Explored, sectionWithExploredPlacemarks.Name);
            Assert.AreEqual(1, sectionWithExploredPlacemarks.Groups.Count);
            Assert.AreEqual(1, sectionWithExploredPlacemarks.Groups[0].Placemarks.Count);
            Assert.AreEqual(placemarkExplored, sectionWithExploredPlacemarks.Groups[0].Placemarks[0]);
        }
    }
}
