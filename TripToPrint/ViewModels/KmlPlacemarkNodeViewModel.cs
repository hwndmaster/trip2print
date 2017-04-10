using System.Collections.ObjectModel;

using TripToPrint.Core.Models;

namespace TripToPrint.ViewModels
{
    public class KmlPlacemarkNodeViewModel : KmlObjectTreeNodeViewModel
    {
        private readonly KmlPlacemark _placemark;
        private DiscoveredPlace _selectedDiscoveredPlace;
        private bool _discoveredPlacesLoaded;

        public KmlPlacemarkNodeViewModel(KmlPlacemark placemark, KmlFolderNodeViewModel parent)
        {
            _placemark = placemark;
            Parent = parent;
        }

        public override string Name => _placemark.Name;
        public override IKmlElement Element => _placemark;

        public DiscoveredPlace SelectedDiscoveredPlace
        {
            get { return _selectedDiscoveredPlace; }
            set
            {
                if (value == _selectedDiscoveredPlace)
                    return;
                _selectedDiscoveredPlace = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DiscoveredPlace> DiscoveredPlaces { get; } = new ObservableCollection<DiscoveredPlace>();

        public bool DiscoveredPlacesLoaded
        {
            get { return _discoveredPlacesLoaded; }
            set
            {
                if (value == _discoveredPlacesLoaded)
                    return;
                _discoveredPlacesLoaded = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(DiscoveredPlacesLoadedAndAvailable));
            }
        }

        public bool DiscoveredPlacesLoadedAndAvailable => DiscoveredPlacesLoaded && DiscoveredPlaces?.Count > 0;
    }
}
