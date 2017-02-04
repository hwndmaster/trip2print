using System.Windows;
using System.Windows.Controls;

using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IStepIntroView : IView<IStepIntroPresenter>
    {
    }

    public partial class StepIntro : UserControl, IStepIntroView
    {
        public StepIntro()
        {
            InitializeComponent();
        }

        public IStepIntroPresenter Presenter { get; set; }

        private void LabelInputFile_OnClick(object sender, RoutedEventArgs e)
        {
            Presenter.AskUserToSelectKmzFile();
        }
    }
}
