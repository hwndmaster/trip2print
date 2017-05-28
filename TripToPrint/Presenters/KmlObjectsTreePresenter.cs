using System.Threading;

using TripToPrint.Core;
using TripToPrint.Core.Models;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IKmlObjectsTreePresenter : IPresenter<KmlObjectsTreeViewModel, IKmlObjectsTreeView>
    {
        void HandleActivated(KmlDocument kmlDocument, CancellationToken cancellationToken);
        void SelectAllItemsToInclude(bool select);
    }

    public class KmlObjectsTreePresenter : IKmlObjectsTreePresenter
    {
        private readonly IKmlCalculator _kmlCalculator;

        public KmlObjectsTreePresenter(IKmlCalculator kmlCalculator)
        {
            _kmlCalculator = kmlCalculator;
        }

        public IKmlObjectsTreeView View { get; private set; }
        public virtual KmlObjectsTreeViewModel ViewModel { get; private set; }

        public void InitializePresenter(IKmlObjectsTreeView view, KmlObjectsTreeViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new KmlObjectsTreeViewModel();

            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;
        }

        public void HandleActivated(KmlDocument kmlDocument, CancellationToken cancellationToken)
        {
            ReadKmlDocumentIntoViewModel(kmlDocument);
        }

        public void SelectAllItemsToInclude(bool select)
        {
            foreach (var folderVm in ViewModel.FoldersToInclude)
            {
                folderVm.Enabled = select;
                foreach (var placemarkVm in folderVm.Children)
                {
                    placemarkVm.Enabled = select;
                }
            }
        }

        private void ReadKmlDocumentIntoViewModel(KmlDocument document)
        {
            ViewModel.FoldersToInclude.Clear();
            foreach (var folder in document.Folders)
            {
                var isRoute = _kmlCalculator.CompleteFolderIsRoute(folder);
                var folderVm = new KmlFolderNodeViewModel(folder) { Enabled = true, IsPartOfRoute = isRoute };
                foreach (var placemark in folder.Placemarks)
                {
                    folderVm.Children.Add(new KmlPlacemarkNodeViewModel(placemark, folderVm)
                    {
                        Enabled = true,
                        IsPartOfRoute = isRoute
                    });
                }
                ViewModel.FoldersToInclude.Add(folderVm);
            }
        }
    }
}
