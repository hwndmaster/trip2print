using System.Diagnostics.CodeAnalysis;
using System.Windows;

using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IKmlObjectsTreeView : IView<IKmlObjectsTreePresenter>
    {
    }

    [ExcludeFromCodeCoverage]
    public partial class KmlObjectsTreeView : IKmlObjectsTreeView
    {
        public KmlObjectsTreeView()
        {
            InitializeComponent();
        }

        public IKmlObjectsTreePresenter Presenter { get; set; }

        private void SelectNone_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.SelectAllItemsToInclude(false);
        }

        private void SelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.SelectAllItemsToInclude(true);
        }
    }
}
