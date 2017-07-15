using System;
using System.Windows;

using TripToPrint.ViewModels;

namespace TripToPrint.Views
{
    public partial class WizardStepButton
    {
        public event EventHandler<WizardStepButtonViewModel> Click;

        public WizardStepButton()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, DataContext as WizardStepButtonViewModel);

            e.Handled = false;
        }
    }
}
