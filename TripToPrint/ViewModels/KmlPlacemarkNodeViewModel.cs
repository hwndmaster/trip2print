using TripToPrint.Core.Models;

namespace TripToPrint.ViewModels
{
    public class KmlPlacemarkNodeViewModel : KmlObjectTreeNodeViewModel
    {
        private readonly KmlPlacemark _placemark;

        public KmlPlacemarkNodeViewModel(KmlPlacemark placemark, KmlObjectTreeNodeViewModel parent)
        {
            _placemark = placemark;
            Parent = parent;
        }

        public override string Name => _placemark.Name;
        public override IKmlElement Element => _placemark;
    }
}
