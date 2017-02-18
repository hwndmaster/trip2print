using System;
using System.Windows;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepAdjustmentView : IView<IStepAdjustmentPresenter>
    {
        void SetAddress(string address);
    }

    public partial class StepAdjustmentView : IStepAdjustmentView
    {
        public StepAdjustmentView()
        {
            InitializeComponent();
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
    }
}
