using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepAdjustmentView : IView<IStepAdjustmentPresenter>
    {
        void SetAddress(string address);
    }

    [ExcludeFromCodeCoverage]
    public partial class StepAdjustmentView : IStepAdjustmentView
    {
        public StepAdjustmentView()
        {
            InitializeComponent();

            browser.AllowDrop = false;
            browser.Navigating += Browser_Navigating;
        }

        public IStepAdjustmentPresenter Presenter { get; set; }

        public void SetAddress(string address)
        {
            browser.Source = new Uri(address);
        }

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

        private void Browser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (!e.Uri.IsFile)
            {
                e.Cancel = true;
            }
        }
    }
}
