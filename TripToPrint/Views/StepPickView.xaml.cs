using System.Diagnostics.CodeAnalysis;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepPickView : IView<IStepPickPresenter>
    {
        IKmlObjectsTreeView KmlObjectsTreeView { get; }
    }

    [ExcludeFromCodeCoverage]
    public partial class StepPickView : IStepPickView
    {
        public StepPickView()
        {
            InitializeComponent();
        }

        public IStepPickPresenter Presenter { get; set; }
        public IKmlObjectsTreeView KmlObjectsTreeView => kmlObjectsTreeViewCtrl;
    }
}
