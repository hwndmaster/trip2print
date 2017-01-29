using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TripToPrint.ViewModels
{
    public class MainFormViewModel : INotifyPropertyChanged
    {
        private int _progressInPercentage;
        private string _selectedInputFileName;
        private string _outputFileName;

        public string SelectedInputFileName
        {
            get { return _selectedInputFileName; }
            set
            {
                if (value == _selectedInputFileName)
                    return;
                _selectedInputFileName = value;
                OnPropertyChanged();
            }
        }

        public string OutputFileName
        {
            get { return _outputFileName; }
            set
            {
                if (value == _outputFileName)
                    return;
                _outputFileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasOutputFileName));
            }
        }

        public bool HasOutputFileName => !string.IsNullOrEmpty(_outputFileName);

        public int ProgressInPercentage
        {
            get { return _progressInPercentage; }
            set
            {
                if (value == _progressInPercentage)
                    return;
                _progressInPercentage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
