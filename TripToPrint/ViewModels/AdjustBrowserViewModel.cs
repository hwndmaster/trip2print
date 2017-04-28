namespace TripToPrint.ViewModels
{
    public class AdjustBrowserViewModel : ViewModelBase
    {
        private string _address;

        public string Address
        {
            get { return _address; }
            set
            {
                if (value == _address)
                    return;
                _address = value;
                OnPropertyChanged();
            }
        }
    }
}
