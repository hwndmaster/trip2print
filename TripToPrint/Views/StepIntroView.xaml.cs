﻿using System.Linq;
using System.Threading.Tasks;
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

        private async void OnInputSourceDrop(object sender, DragEventArgs e)
        {
            var supportedFormats = new [] {
                DataFormats.FileDrop, "UniformResourceLocator"
            };

            if (supportedFormats.Any(x => e.Data.GetDataPresent(x)))
            {
                await Presenter.HandleInputUriDrop(e.Data);
            }
        }
    }
}
