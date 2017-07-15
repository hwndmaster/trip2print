using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using TripToPrint.Presenters;
using TripToPrint.ViewModels;

namespace TripToPrint.Views
{
    public interface IMainWindowView : IView<IMainWindowPresenter>
    {
        IStepIntroView StepIntroView { get; }
        IStepPickView StepPickView { get; }
        IStepInProgressView StepDiscoveringView { get; }
        IStepExploreView StepExploreView { get; }
        IStepInProgressView StepGenerationView { get; }
        IStepTuningView StepTuningView { get; }

        void Show();
    }

    [ExcludeFromCodeCoverage]
    public sealed partial class MainWindow : IMainWindowView
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public IMainWindowPresenter Presenter { get; set; }
        public IStepIntroView StepIntroView => stepIntro;
        public IStepPickView StepPickView => stepPick;
        public IStepInProgressView StepDiscoveringView => stepDiscovering;
        public IStepExploreView StepExploreView => stepExplore;
        public IStepInProgressView StepGenerationView => stepGeneration;
        public IStepTuningView StepTuningView => stepTuning;

        private void TabWizard_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string back = "Back", next = "Next";
            Presenter.GetBackNextTitlesForCurrentStep(ref back, ref next);

            buttonBack.Content = back;
            buttonNext.Content = next;
        }

        private async void ButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                buttonBack.IsEnabled = buttonNext.IsEnabled = false;
                await Presenter.GoBack();
                buttonBack.IsEnabled = buttonNext.IsEnabled = true;
            }
        }

        private async void ButtonNext_OnClick(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                buttonBack.IsEnabled = buttonNext.IsEnabled = false;
                await Presenter.GoNext();
                buttonBack.IsEnabled = buttonNext.IsEnabled = true;
            }
        }

        private void LinkGithub_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Presenter.BrowseUrl(e.Uri);
        }

        private async void WizardStepButton_OnClick(object sender, WizardStepButtonViewModel e)
        {
            await Presenter.GoToStep(e.Index - 1);
        }
    }
}
