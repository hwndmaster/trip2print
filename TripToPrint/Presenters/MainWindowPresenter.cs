using System;
using System.Diagnostics;
using System.Threading.Tasks;

using TripToPrint.Core.Logging;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IMainWindowPresenter : IPresenter<MainWindowViewModel, IMainWindowView>
    {
        void OpenReport();
        void OpenReportContainingFolder();
        Task GoBack();
        Task GoNext();
        void GetBackNextTitlesForCurrentStep(ref string back, ref string next);
    }

    public class MainWindowPresenter : IMainWindowPresenter
    {
        private readonly IStepIntroPresenter _stepIntroPresenter;
        private readonly IStepGenerationPresenter _stepGenerationPresenter;
        private readonly ILogger _logger;

        public MainWindowPresenter(IStepIntroPresenter stepIntroPresenter, IStepGenerationPresenter stepGenerationPresenter, ILogger logger)
        {
            _stepIntroPresenter = stepIntroPresenter;
            _stepGenerationPresenter = stepGenerationPresenter;
            _logger = logger;
        }


        public IMainWindowView View { get; private set; }
        public MainWindowViewModel ViewModel { get; private set; }

        public void InitializePresenter(IMainWindowView view, MainWindowViewModel viewModel)
        {
            ViewModel = viewModel ?? new MainWindowViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            _stepIntroPresenter.InitializePresenter(View.StepIntroView, ViewModel.StepIntro);
            _stepGenerationPresenter.InitializePresenter(View.StepGenerationView, ViewModel.StepGeneration);

            // TODO: Convert to Rx observable
            ViewModel.StepIntro.InputUriChanged += (sender, inputUri) => {
                _logger.Info($"Input file selected: {inputUri}");
                ViewModel.StepGeneration.InputUri = inputUri;
            };
            ViewModel.StepIntro.OutputFileNameChanged += (sender, outputFileName) => {
                _logger.Info($"Output file selected: {outputFileName}");
                ViewModel.StepGeneration.OutputFileName = outputFileName;
            };

            GetWizardStepPresenter(ViewModel.WizardStepIndex).Activated().GetAwaiter().GetResult();
        }

        // TODO: Use this method on the last wizard step
        public void OpenReport()
        {
            Process.Start(ViewModel.StepIntro.OutputFileName);
        }

        // TODO: Use this method on the last wizard step
        public void OpenReportContainingFolder()
        {
            string argument = "/select, \"" + ViewModel.StepIntro.OutputFileName + "\"";
            Process.Start("explorer.exe", argument);
        }

        public async Task GoBack()
        {
            var currentStepPresenter = GetWizardStepPresenter(ViewModel.WizardStepIndex);
            if (currentStepPresenter.ValidateToGoBack())
            {
                ViewModel.WizardStepIndex--;

                await GetWizardStepPresenter(ViewModel.WizardStepIndex).Activated();
            }
        }

        public async Task GoNext()
        {
            var currentStepPresenter = GetWizardStepPresenter(ViewModel.WizardStepIndex);
            if (currentStepPresenter.ValidateToGoNext())
            {
                ViewModel.WizardStepIndex++;

                await GetWizardStepPresenter(ViewModel.WizardStepIndex).Activated();
            }
        }

        public void GetBackNextTitlesForCurrentStep(ref string back, ref string next)
        {
            var currentStepPresenter = GetWizardStepPresenter(ViewModel.WizardStepIndex);
            currentStepPresenter.GetBackNextTitles(ref back, ref next);
        }

        private IStepPresenter GetWizardStepPresenter(int wizardStepIndex)
        {
            switch (wizardStepIndex)
            {
                case 0:
                    return _stepIntroPresenter;
                case 1:
                    return _stepGenerationPresenter;
            }

            throw new NotSupportedException($"WizardStepIndex is invalid: {wizardStepIndex}");
        }
    }
}
