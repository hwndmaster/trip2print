using System.Diagnostics.CodeAnalysis;
using System.Windows;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepTuningView : IView<IStepTuningPresenter>
    {
        ITuningBrowserView TuningBrowserView { get; }
    }

    [ExcludeFromCodeCoverage]
    public sealed partial class StepTuningView : IStepTuningView
    {

        public StepTuningView()
        {
            InitializeComponent();
        }

        public IStepTuningPresenter Presenter { get; set; }
        public ITuningBrowserView TuningBrowserView => tuningBrowser;

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
