using System.Diagnostics.CodeAnalysis;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepSettingView : IView<IStepSettingPresenter>
    {
        IKmlObjectsTreeView KmlObjectsTreeView { get; }
    }

    [ExcludeFromCodeCoverage]
    public partial class StepSettingView : IStepSettingView
    {
        public StepSettingView()
        {
            InitializeComponent();
        }

        public IStepSettingPresenter Presenter { get; set; }
        public IKmlObjectsTreeView KmlObjectsTreeView => kmlObjectsTreeViewCtrl;
    }
}
