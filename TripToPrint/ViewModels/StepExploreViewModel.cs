using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TripToPrint.ViewModels
{
    public class StepExploreViewModel : ViewModelBase
    {
        public List<DiscoveredSectionViewModel> Sections { get; set; }

        public ObservableCollection<DiscoveredGroupViewModel> GetUpperGroupForMatchingPlacemarks() => Sections[0].Groups;
        public ObservableCollection<DiscoveredGroupViewModel> GetUpperGroupForExploring() => Sections[1].Groups;
    }
}
