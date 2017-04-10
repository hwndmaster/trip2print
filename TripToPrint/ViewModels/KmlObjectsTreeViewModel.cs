using System.Collections.ObjectModel;
using System.Linq;

namespace TripToPrint.ViewModels
{
    public class KmlObjectsTreeViewModel : ViewModelBase
    {
        private bool _discoveringIsDone;
        private bool _discoveringIsDoneAndAvailable;

        public ObservableCollection<KmlObjectTreeNodeViewModel> FoldersToInclude { get; }
            = new ObservableCollection<KmlObjectTreeNodeViewModel>();

        public bool DiscoveringIsDone
        {
            get { return _discoveringIsDone; }
            set
            {
                if (value == _discoveringIsDone)
                    return;
                _discoveringIsDone = value;
                OnPropertyChanged();

                UpdateAvailability();
            }
        }

        private void UpdateAvailability()
        {
            DiscoveringIsDoneAndAvailable = DiscoveringIsDone
                && FoldersToInclude.SelectMany(x => x.Children.OfType<KmlPlacemarkNodeViewModel>()).Any(x => x.DiscoveredPlacesLoadedAndAvailable);
        }

        public bool DiscoveringIsDoneAndAvailable
        {
            get { return _discoveringIsDoneAndAvailable; }
            set
            {
                if (value == _discoveringIsDoneAndAvailable)
                    return;
                _discoveringIsDoneAndAvailable = value;
                OnPropertyChanged();
            }
        }
    }
}
