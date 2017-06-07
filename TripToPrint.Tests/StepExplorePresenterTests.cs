using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core.Models;
using TripToPrint.Presenters;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Tests
{
    [TestClass]
    public class StepExplorePresenterTests
    {
        private readonly Mock<IStepExploreView> _viewMock = new Mock<IStepExploreView>();
        private readonly Mock<IUserSession> _userSessionMock = new Mock<IUserSession>();
        private readonly Mock<IProcessService> _processMock = new Mock<IProcessService>();

        private Mock<StepExplorePresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepExplorePresenter>(
                _userSessionMock.Object,
                _processMock.Object) {
                CallBase = true
            };
        }

        [TestMethod]
        public void When_initializing_presenter_the_properties_are_set_correctly()
        {
            // Act
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Verify
            Assert.AreEqual(_viewMock.Object, _presenter.Object.View);
            Assert.IsNotNull(_presenter.Object.ViewModel);
            _viewMock.VerifySet(x => x.Presenter = _presenter.Object);
            _viewMock.VerifySet(x => x.DataContext = _presenter.Object.ViewModel);
        }

        [TestMethod]
        public async Task When_activated_the_viewmodel_is_filled_up()
        {
            // Arrange
            var vm = InitializeAndCreateViewModel();
            var discoveredPlaces = new List<DiscoveredPlace> {
                new DiscoveredPlace { Venue = new DummyVenue(), AttachedToPlacemark = new KmlPlacemark { Name = "pm-1" } },
                new DiscoveredPlace { Venue = new DummyVenue(), AttachedToPlacemark = new KmlPlacemark { Name = "pm-1" } },
                new DiscoveredPlace { Venue = new DummyVenue { Title = "venue-1", Region = "region-1" } },
                new DiscoveredPlace { Venue = new DummyVenue { Title = "venue-2", Region = "region-1" } },
                new DiscoveredPlace { Venue = new DummyVenue { Title = "venue-3", Region = "region-2" } }
            };
            _userSessionMock.SetupGet(x => x.DiscoveredVenues).Returns(discoveredPlaces);

            // Act
            await _presenter.Object.Activated();

            // Verify
            var matching = vm.GetUpperGroupForMatchingPlacemarks();
            Assert.AreEqual(1, matching.Count);
            Assert.AreEqual("pm-1", matching[0].Name);
            Assert.AreEqual(discoveredPlaces[0].AttachedToPlacemark, matching[0].AttachedPlacemark);
            AssertDiscoveredPlaces(new[] { discoveredPlaces[0], discoveredPlaces[1] }, matching[0].Venues);

            var exploring = vm.GetUpperGroupForExploring();
            Assert.AreEqual(2, exploring.Count);
            Assert.AreEqual("region-1", exploring[0].Name);
            AssertDiscoveredPlaces(new [] { discoveredPlaces[2], discoveredPlaces[3] }, exploring[0].Venues);
            Assert.IsNull(exploring[0].AttachedPlacemark);
            Assert.AreEqual("region-2", exploring[1].Name);
            AssertDiscoveredPlaces(new[] { discoveredPlaces[4] }, exploring[1].Venues);
            Assert.IsNull(exploring[1].AttachedPlacemark);
            Assert.AreEqual(1, exploring[1].Venues.Count);
        }

        [TestMethod]
        public async Task When_filling_up_the_viewmodel_the_previously_enabled_items_remained_enabled()
        {
            // Arrange
            var vm = InitializeAndCreateViewModel();

            vm.GetUpperGroupForMatchingPlacemarks().Add(new DiscoveredGroupViewModel
            {
                AttachedPlacemark = new KmlPlacemark { Name = "pm-1" },
                Venues = {
                    new DiscoveredVenueViewModel(new DummyVenue { Title = "venue-1" }) { Enabled = true }
                }
            });
            vm.GetUpperGroupForExploring().Add(new DiscoveredGroupViewModel
            {
                Venues = {
                    new DiscoveredVenueViewModel(new DummyVenue { Title = "venue-4", Region = "region-2" }) { Enabled = true }
                }
            });

            var discoveredPlaces = new List<DiscoveredPlace> {
                new DiscoveredPlace { Venue = new DummyVenue { Title = "venue-1" }, AttachedToPlacemark = new KmlPlacemark { Name = "pm-1" } },
                new DiscoveredPlace { Venue = new DummyVenue { Title = "venue-2" }, AttachedToPlacemark = new KmlPlacemark { Name = "pm-2" } },
                new DiscoveredPlace { Venue = new DummyVenue { Title = "venue-3", Region = "region-1" } },
                new DiscoveredPlace { Venue = new DummyVenue { Title = "venue-4", Region = "region-2" } }
            };
            _userSessionMock.SetupGet(x => x.DiscoveredVenues).Returns(discoveredPlaces);

            // Act
            await _presenter.Object.Activated();

            // Verify
            var matching = vm.GetUpperGroupForMatchingPlacemarks();
            Assert.IsTrue(matching[0].Venues[0].Enabled);
            Assert.IsFalse(matching[1].Venues[0].Enabled);

            var exploring = vm.GetUpperGroupForExploring();
            Assert.IsFalse(exploring[0].Venues[0].Enabled);
            Assert.IsTrue(exploring[1].Venues[0].Enabled);
        }

        [TestMethod]
        public async Task When_going_next_the_session_is_updated_and_enabled_discovered_places_are_set()
        {
            // Arrange
            var includedVenues = new List<DiscoveredPlace> {
                new DiscoveredPlace()
            };
            _userSessionMock.SetupGet(x => x.IncludedVenues).Returns(includedVenues);

            var matchingPlacemark = new KmlPlacemark();
            var matchingVenueVm = new DiscoveredVenueViewModel(new DummyVenue()) { Enabled = true };
            var exploringVenueVm = new DiscoveredVenueViewModel(new DummyVenue()) { Enabled = true };
            var vm = InitializeAndCreateViewModel();

            vm.GetUpperGroupForMatchingPlacemarks().Add(new DiscoveredGroupViewModel {
                AttachedPlacemark = matchingPlacemark,
                Venues = {
                    new DiscoveredVenueViewModel(new DummyVenue()) { Enabled = false },
                    matchingVenueVm
                }
            });
            vm.GetUpperGroupForExploring().Add(new DiscoveredGroupViewModel
            {
                Venues = {
                    new DiscoveredVenueViewModel(new DummyVenue()) { Enabled = false },
                    exploringVenueVm
                }
            });

            // Act
            await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(2, includedVenues.Count);
            Assert.AreEqual(matchingVenueVm.Venue, includedVenues[0].Venue);
            Assert.AreEqual(matchingPlacemark, includedVenues[0].AttachedToPlacemark);
            Assert.AreEqual(exploringVenueVm.Venue, includedVenues[1].Venue);
            Assert.IsNull(includedVenues[1].AttachedToPlacemark);
        }

        [TestMethod]
        public void SelectAll_with_true_argument_enables_all_items()
        {
            TestSelectAllWithArgument(true);
        }

        [TestMethod]
        public void SelectAll_with_false_argument_disables_all_items()
        {
            TestSelectAllWithArgument(false);
        }

        private void TestSelectAllWithArgument(bool enabled)
        {
            // Arrange
            var vm = InitializeAndCreateViewModel();

            foreach (var section in vm.Sections)
            {
                for (var i = 0; i < 10; i++)
                {
                    section.Groups.Add(new DiscoveredGroupViewModel
                    {
                        Venues = {
                            new DiscoveredVenueViewModel(new DummyVenue()),
                            new DiscoveredVenueViewModel(new DummyVenue())
                        }
                    });
                }
            }

            // Act
            _presenter.Object.SelectAll(enabled);

            // Verify
            vm.Sections.ForEach(x => x.Groups.ToList()
                .ForEach(g => g.Venues.ToList()
                .ForEach(v => Assert.AreEqual(enabled, v.Enabled))));
        }

        private StepExploreViewModel InitializeAndCreateViewModel()
        {
            var vm = new StepExploreViewModel();

            _presenter.Object.InitializePresenter(_viewMock.Object, vm);

            _presenter.SetupGet(x => x.ViewModel).Returns(vm);
            return vm;
        }

        private static void AssertDiscoveredPlaces(IReadOnlyList<DiscoveredPlace> discoveredPlaces, IReadOnlyList<DiscoveredVenueViewModel> venues)
        {
            Assert.AreEqual(discoveredPlaces.Count, venues.Count);
            for (var i = 0; i < discoveredPlaces.Count; i++)
            {
                Assert.AreEqual(discoveredPlaces[i].Venue, venues[i].Venue);
            }
        }
    }
}
