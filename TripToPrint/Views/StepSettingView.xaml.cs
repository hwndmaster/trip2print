using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepSettingView : IView<IStepSettingPresenter>
    {
    }

    public partial class StepSettingView : IStepSettingView
    {
        public StepSettingView()
        {
            InitializeComponent();
        }

        public IStepSettingPresenter Presenter { get; set; }
    }
}
