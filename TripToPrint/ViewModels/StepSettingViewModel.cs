using System;
using System.Collections.ObjectModel;

namespace TripToPrint.ViewModels
{
    public class StepSettingViewModel : ViewModelBase
    {
        private string _inputFileName;

        public string InputFileName
        {
            get { return _inputFileName; }
            set
            {
                if (value == _inputFileName)
                    return;
                _inputFileName = value;
                OnPropertyChanged();

                InputFileNameChanged?.Invoke(this, value);
            }
        }

        public ObservableCollection<KmlObjectTreeNodeViewModel> FoldersToInclude { get; } = new ObservableCollection<KmlObjectTreeNodeViewModel>();

        public event EventHandler<string> InputFileNameChanged;
    }
}
