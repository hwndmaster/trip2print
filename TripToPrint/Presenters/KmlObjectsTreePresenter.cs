using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using TripToPrint.Core;
using TripToPrint.Core.Models;
using TripToPrint.Services;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IKmlObjectsTreePresenter : IPresenter<KmlObjectsTreeViewModel, IKmlObjectsTreeView>
    {
        void HandleActivated(KmlDocument kmlDocument, CancellationToken cancellationToken);
        void SelectAllItemsToInclude(bool select);
        void SelectBestMatchingDiscoveredPlaces(bool select);
    }

    public class KmlObjectsTreePresenter : IKmlObjectsTreePresenter
    {
        private readonly IKmlCalculator _kmlCalculator;
        private readonly IHereAdapter _hereAdapter;
        private readonly IUserSession _userSession;

        public KmlObjectsTreePresenter(IKmlCalculator kmlCalculator, IHereAdapter hereAdapter, IUserSession userSession)
        {
            _kmlCalculator = kmlCalculator;
            _hereAdapter = hereAdapter;
            _userSession = userSession;
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
            ViewModel.DiscoveringIsDone = false;

            ReadKmlDocumentIntoViewModel(kmlDocument);

            StartDiscoveringPlaces(cancellationToken);
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

        public void SelectBestMatchingDiscoveredPlaces(bool select)
        {
            foreach (var folderVm in ViewModel.FoldersToInclude)
            {
                foreach (var placemarkVm in folderVm.Children.OfType<KmlPlacemarkNodeViewModel>())
                {
                    if (select && placemarkVm.DiscoveredPlacesLoadedAndAvailable)
                    {
                        placemarkVm.SelectedDiscoveredPlace = placemarkVm.DiscoveredPlaces[0];
                    }
                    else
                    {
                        placemarkVm.SelectedDiscoveredPlace = null;
                    }
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

        private void StartDiscoveringPlaces(CancellationToken cancellationToken)
        {
            // TODO: Write unit test to cover this method
            Task.Factory.StartNew(async () => {
                var placemarksToDiscover = ViewModel.FoldersToInclude
                    .Where(x => !x.IsPartOfRoute)
                    .SelectMany(x => x.Children)
                    .OfType<KmlPlacemarkNodeViewModel>();

                var degreeOfParallelism = 4;
                await placemarksToDiscover.ForEachAsync(degreeOfParallelism, async (placemarkNodeVm) => {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    var placemark = placemarkNodeVm.Element as KmlPlacemark;
                    var discoveredPlaces = await _hereAdapter.LookupPlaces(placemark, _userSession.UserLanguage, cancellationToken);
                    if (discoveredPlaces != null && Application.Current != null)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() => {
                            foreach (var discoveredPlace in discoveredPlaces)
                            {
                                placemarkNodeVm.DiscoveredPlaces.Add(discoveredPlace);
                            }
                        });
                    }
                    placemarkNodeVm.DiscoveredPlacesLoaded = true;
                });

                ViewModel.DiscoveringIsDone = true;
            }, cancellationToken);
        }
    }
}
