using TripToPrint.Core.Models;

namespace TripToPrint.ViewModels
{
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
}
