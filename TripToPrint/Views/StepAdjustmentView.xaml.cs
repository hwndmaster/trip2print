using System;
using System.Windows.Controls;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepAdjustmentView : IView<IStepAdjustmentPresenter>
    {
        void SetAddress(string address);
    }

    public partial class StepAdjustmentView : UserControl, IStepAdjustmentView
    {
        // TODO: Add "Open report file" button on the last wizard step
        //       Presenter.OpenReport();
        // TODO: Add "Open report containing folder" button on the last wizard step
        //       Presenter.OpenReportContainingFolder();
        // TODO: Add "Copy report path to clipboard" button on the last wizard step
        //       Clipboard.SetText(ViewModel.OutputFileName, TextDataFormat.Text);

        public StepAdjustmentView()
        {
            InitializeComponent();
        }

        public IStepAdjustmentPresenter Presenter { get; set; }

        public void SetAddress(string address)
        {
            browser.Source = new Uri(address);
        }
    }
}
