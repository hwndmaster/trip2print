using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IMainWindowView : IView<IMainWindowPresenter>
    {
        IStepIntroView StepIntroView { get; }
        IStepGenerationView StepGenerationView { get; }

        void Show();
    }

    public partial class MainWindow : Window, IMainWindowView
    {
        // TODO: Add "Open report file" button on the last wizard step
        //       Presenter.OpenReport();
        // TODO: Add "Open report containing folder" button on the last wizard step
        //       Presenter.OpenReportContainingFolder();
        // TODO: Add "Copy report path to clipboard" button on the last wizard step
        //       Clipboard.SetText(ViewModel.OutputFileName, TextDataFormat.Text);

        public MainWindow()
        {
            InitializeComponent();
        }

        public IMainWindowPresenter Presenter { get; set; }
        public IStepIntroView StepIntroView => stepIntro;
        public IStepGenerationView StepGenerationView => stepGeneration;

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
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }
    }
}
