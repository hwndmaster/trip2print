using System.Diagnostics.CodeAnalysis;
using System.Windows;

using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepExploreView : IView<IStepExplorePresenter>
    {
    }

    [ExcludeFromCodeCoverage]
    public partial class StepExploreView : IStepExploreView
    {
        public StepExploreView()
        {
            InitializeComponent();
        }

        public IStepExplorePresenter Presenter { get; set; }

        private void SelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.SelectAll(true);
        }

        private void SelectNone_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.SelectAll(false);
        }

        private void SelectBest_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.SelectBest();
        }
    }
}
