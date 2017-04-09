using System.Collections.ObjectModel;
using TripToPrint.Core.Models;

namespace TripToPrint.ViewModels
{
    public abstract class KmlObjectTreeNodeViewModel : ViewModelBase
    {
        private bool _enabled;

        public abstract string Name { get; }
        public abstract IKmlElement Element { get; }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled)
                    return;
                _enabled = value;
                OnPropertyChanged();

                foreach (var child in Children)
                {
                    child.Enabled = value;
                }
            }
        }

        public ObservableCollection<KmlObjectTreeNodeViewModel> Children { get; } = new ObservableCollection<KmlObjectTreeNodeViewModel>();
    }

    public class KmlFolderNodeViewModel : KmlObjectTreeNodeViewModel
    {
        private readonly KmlFolder _folder;

        public KmlFolderNodeViewModel(KmlFolder folder)
        {
            _folder = folder;
        }

        public override string Name => _folder.Name;
        public override IKmlElement Element => _folder;
    }

    public class KmlPlacemarkNodeViewModel : KmlObjectTreeNodeViewModel
    {
        private readonly KmlPlacemark _placemark;

        public KmlPlacemarkNodeViewModel(KmlPlacemark placemark)
        {
            _placemark = placemark;
        }

        public override string Name => _placemark.Name;
        public override IKmlElement Element => _placemark;
    }
}
