using System.Windows.Controls;

using TripToPrint.Core.Logging;
using TripToPrint.Presenters;
using TripToPrint.ViewModels;

namespace TripToPrint.Views
{
    public interface IStepGenerationView : IView<IStepGenerationPresenter>
    {
        void AddLogItem(LogItem item);
        void ClearLogItems();
    }

    public partial class StepGenerationView : UserControl, IStepGenerationView
    {
        public StepGenerationView()
        {
            InitializeComponent();
        }

        public IStepGenerationPresenter Presenter { get; set; }

        public void AddLogItem(LogItem item)
        {
            listLog.Items.Add(new LogItemViewModel(item));

            listLog.ScrollIntoView(listLog.Items[listLog.Items.Count - 1]);
        }

        public void ClearLogItems()
        {
            listLog.Items.Clear();
        }
    }
}
