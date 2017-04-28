using System.Diagnostics.CodeAnalysis;
using System.Windows;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepAdjustmentView : IView<IStepAdjustmentPresenter>
    {
        IAdjustBrowserView AdjustBrowserView { get; }
    }

    [ExcludeFromCodeCoverage]
    public partial class StepAdjustmentView : IStepAdjustmentView
    {

        public StepAdjustmentView()
        {
            InitializeComponent();
        }

        public IStepAdjustmentPresenter Presenter { get; set; }
        public IAdjustBrowserView AdjustBrowserView => adjustBrowser;

        private void OpenReport_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.OpenReport();
        }

        private void OpenContainingFolder_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.OpenReportContainingFolder();
        }

        private void CopyPath_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.CopyReportPathToClipboard();
        }
    }
}
