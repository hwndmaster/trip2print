using TripToPrint.ReportTuning.Dto;

namespace TripToPrint.ViewModels
{
    public class TuningBrowserViewModel : ViewModelBase
    {
        private MooiDocumentDto _document;

        public MooiDocumentDto Document
        {
            get { return _document; }
            set
            {
                if (value == _document)
                    return;
                _document = value;
                OnPropertyChanged();
            }
        }
    }
}
