using System;
using System.Collections.Generic;

namespace TripToPrint.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            StepIntro = new StepIntroViewModel();
            StepPick = new StepPickViewModel();
            StepDiscovering = new StepInProgressViewModel();
            StepExplore = new StepExploreViewModel();
            StepGeneration = new StepInProgressViewModel();
            StepTuning = new StepTuningViewModel();

            StepButtons = new List<WizardStepButtonViewModel> {
                new WizardStepButtonViewModel { Index = 1, Title = "Source selection", IsFirst = true },
                new WizardStepButtonViewModel { Index = 2, Title = "Pick placemarks" },
                new WizardStepButtonViewModel { Index = 3, Title = "Discovering\r\nrecommendations" },
                new WizardStepButtonViewModel { Index = 4, Title = "Explore" },
                new WizardStepButtonViewModel { Index = 5, Title = "Generating" },
                new WizardStepButtonViewModel { Index = 6, Title = "Tuning", IsLast = true }
            };

            WizardStepIndexChanged += (sender, wizardStepIndex) => HandleWizardStepIndexChanged();
        }

        public StepIntroViewModel StepIntro { get; }
        public StepPickViewModel StepPick { get; }
        public StepInProgressViewModel StepDiscovering { get; }
        public StepExploreViewModel StepExplore { get; }
        public StepInProgressViewModel StepGeneration { get; }
        public StepTuningViewModel StepTuning { get; }

        public List<WizardStepButtonViewModel> StepButtons { get; }

        public EventHandler<int> WizardStepIndexChanged;

        public int WizardStepIndex
        {
            get => GetOrDefault<int>();
            set => RaiseAndSetIfChanged(value, WizardStepIndexChanged);
        }

        public bool CanGoBack => WizardStepIndex > 0;

        private void HandleWizardStepIndexChanged()
        {
            OnPropertyChanged(nameof(CanGoBack));
        }
    }
}
