using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

using TripToPrint.Core.Logging;
using TripToPrint.Presenters;
using TripToPrint.ViewModels;

namespace TripToPrint.Views
{
    public interface IMainFormView : IView<IMainFormPresenter, MainFormViewModel>
    {
        void AddLogItem(LogItem item);
        void ClearLogItems();
    }

    public partial class MainFormView : Form, IMainFormView
    {
        public MainFormView()
        {
            InitializeComponent();
        }

        public IMainFormPresenter Presenter { get; set; }
        public MainFormViewModel ViewModel { get; set; }

        public void BindData()
        {
            linkKmzFile.DataBindings.Add(nameof(linkKmzFile.Text), ViewModel,
                nameof(ViewModel.SelectedInputFileName), true, DataSourceUpdateMode.OnPropertyChanged,
                "(click here to select)");

            progressBar.DataBindings.Add(nameof(progressBar.Value), ViewModel, nameof(ViewModel.ProgressInPercentage));

            buttonCopyReportPath.DataBindings.Add(nameof(buttonCopyReportPath.Enabled),
                ViewModel, nameof(ViewModel.HasOutputFileName));
            buttonOpenReport.DataBindings.Add(nameof(buttonOpenReport.Enabled),
                ViewModel, nameof(ViewModel.HasOutputFileName));
            buttonOpenContainingFolder.DataBindings.Add(nameof(buttonOpenContainingFolder.Enabled),
                ViewModel, nameof(ViewModel.HasOutputFileName));
        }

        private void linkKmzFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Presenter.AskUserToSelectKmzFile();
        }

        private async void buttonGenerate_Click(object sender, System.EventArgs e)
        {
            buttonGenerate.Enabled = false;
            Cursor = Cursors.WaitCursor;

            await Presenter.GenerateReport();

            buttonGenerate.Enabled = true;
            Cursor = DefaultCursor;
        }

        public void AddLogItem(LogItem item)
        {
            listLog.Items.Add(item);
            listLog.TopIndex = listLog.Items.Count - 1;
        }

        public void ClearLogItems()
        {
            listLog.Items.Clear();
        }

        private void buttonOpenReport_Click(object sender, System.EventArgs e)
        {
            Presenter.OpenReport();
        }

        private void buttonOpenContainingFolder_Click(object sender, System.EventArgs e)
        {
            Presenter.OpenReportContainingFolder();
        }

        private void buttonCopyReportPath_Click(object sender, System.EventArgs e)
        {
            Clipboard.SetText(ViewModel.OutputFileName, TextDataFormat.Text);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/hwndmaster/trip2print");
        }
    }
}
