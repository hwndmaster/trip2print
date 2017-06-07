using System.Diagnostics.CodeAnalysis;

using TripToPrint.Core.Logging;
using TripToPrint.Presenters;
using TripToPrint.ViewModels;

namespace TripToPrint.Views
{
    public interface IStepInProgressView : IView<IStepInProgressPresenter>
    {
        void AddLogItem(LogItem item);
        void ClearLogItems();
    }

    [ExcludeFromCodeCoverage]
    public sealed partial class StepInProgressView : IStepInProgressView
    {
        public StepInProgressView()
        {
            InitializeComponent();
        }

        public IStepInProgressPresenter Presenter { get; set; }

        public void AddLogItem(LogItem item)
        {
            this.Dispatcher.Invoke(() =>
            {
                listLog.Items.Add(new LogItemViewModel(item));

                listLog.ScrollIntoView(listLog.Items[listLog.Items.Count - 1]);
            });
        }

        public void ClearLogItems()
        {
            listLog.Items.Clear();
        }
    }
}
