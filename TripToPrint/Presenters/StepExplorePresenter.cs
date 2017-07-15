using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TripToPrint.Core.Models;
using TripToPrint.Core.Models.Venues;
using TripToPrint.Properties;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IStepExplorePresenter : IPresenter<StepExploreViewModel, IStepExploreView>, IStepPresenter
    {
        void SelectAll(bool enabled);
        void SelectBest();
        void BrowsePlacemarkUrl(Uri uri);
    }

    public class StepExplorePresenter : IStepExplorePresenter
    {
        private readonly IUserSession _userSession;
        private readonly IProcessService _process;

        public StepExplorePresenter(IUserSession userSession, IProcessService process)
        {
            _userSession = userSession;
            _process = process;
        }

        public IStepExploreView View { get; private set; }
        public virtual StepExploreViewModel ViewModel { get; private set; }
        public virtual IMainWindowPresenter MainWindow { get; set; }

        public void InitializePresenter(IStepExploreView view, StepExploreViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepExploreViewModel();

            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            ViewModel.Sections = new List<DiscoveredSectionViewModel> {
                new DiscoveredSectionViewModel { Name = Resources.StepExplore_UpperGroup_Placemarks },
                new DiscoveredSectionViewModel { Name = Resources.StepExplore_UpperGroup_Explore }
            };
        }

        public async Task Activated()
        {
            FillupViewModel();

            if (!ViewModel.GetUpperGroupForMatchingPlacemarks().Any()
                && !ViewModel.GetUpperGroupForExploring().Any())
            {
                await MainWindow.GoNext();
            }
        }

        public Task<bool> BeforeGoBack()
        {
            return Task.FromResult(true);
        }

        public Task<bool> BeforeGoNext()
        {
            UpdateSession();

            return Task.FromResult(true);
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
            next = "Generate";
        }

        public void SelectAll(bool enabled)
        {
            foreach (var section in ViewModel.Sections)
            {
                foreach (var group in section.Groups)
                {
                    foreach (var venue in group.Venues)
                    {
                        venue.Enabled = enabled;
                    }
                }
            }
        }

        public void SelectBest()
        {
            throw new NotImplementedException();
        }

        public void BrowsePlacemarkUrl(Uri uri)
        {
            _process.Start(uri.AbsoluteUri);
        }

        private void FillupViewModel()
        {
            var previouslyEnabled = new HashSet<VenueBase>();

            foreach (var section in ViewModel.Sections)
            {
                foreach (var enabled in section.Groups.SelectMany(x => x.Venues).Where(x => x.Enabled))
                {
                    previouslyEnabled.Add(enabled.Venue);
                }

                section.Groups.Clear();
            }

            var matchingGroup = from dv in _userSession.DiscoveredVenues
                                where dv.AttachedToPlacemark != null
                                group dv by dv.AttachedToPlacemark
                                into g
                                select g;

            var coll = ViewModel.GetUpperGroupForMatchingPlacemarks();
            foreach (var group in matchingGroup)
            {
                var groupvm = new DiscoveredGroupViewModel {
                    Name = group.Key.Name,
                    AttachedPlacemark = group.Key
                };

                foreach (var venue in group)
                {
                    groupvm.Venues.Add(new DiscoveredVenueViewModel(venue.Venue)
                    {
                        Enabled = previouslyEnabled.Contains(venue.Venue)
                    });
                }

                coll.Add(groupvm);
            }

            var exploringGroup = from dv in _userSession.DiscoveredVenues
                                 where dv.AttachedToPlacemark == null
                                 group dv by dv.Venue.Region
                                 into g
                                 select g;

            coll = ViewModel.GetUpperGroupForExploring();
            foreach (var group in exploringGroup)
            {
                var groupvm = new DiscoveredGroupViewModel { Name = group.Key };
                var venues = group.OrderBy(dv => {
                    if (dv.Venue is IHasDistanceToPlacemark dvd)
                    {
                        return dvd.DistanceToPlacemark ?? 0;
                    }
                    return 0;
                });

                foreach (var venue in venues)
                {
                    groupvm.Venues.Add(new DiscoveredVenueViewModel(venue.Venue)
                    {
                        Enabled = previouslyEnabled.Contains(venue.Venue)
                    });
                }

                coll.Add(groupvm);
            }
        }

        private void UpdateSession()
        {
            _userSession.IncludedVenues.Clear();

            foreach (var discovered in ViewModel.GetUpperGroupForMatchingPlacemarks())
            {
                foreach (var venueViewModel in discovered.Venues.Where(x => x.Enabled))
                {
                    _userSession.IncludedVenues.Add(new DiscoveredPlace {
                        Venue = venueViewModel.Venue,
                        AttachedToPlacemark = discovered.AttachedPlacemark
                    });
                }
            }

            foreach (var venueViewModel in ViewModel.GetUpperGroupForExploring().SelectMany(x => x.Venues).Where(x => x.Enabled))
            {
                _userSession.IncludedVenues.Add(new DiscoveredPlace
                {
                    Venue = venueViewModel.Venue
                });
            }
        }
    }
}
