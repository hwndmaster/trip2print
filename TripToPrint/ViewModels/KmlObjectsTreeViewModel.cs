using System.Collections.ObjectModel;

namespace TripToPrint.ViewModels
{
    public class KmlObjectsTreeViewModel : ViewModelBase
    {
        public ObservableCollection<KmlObjectTreeNodeViewModel> FoldersToInclude { get; }
            = new ObservableCollection<KmlObjectTreeNodeViewModel>();
    }
}
