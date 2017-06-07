using TripToPrint.ReportTuning.Dto;

namespace TripToPrint.ViewModels
{
    public class TuningBrowserViewModel : ViewModelBase
    {
        public MooiDocumentDto Document
        {
            get => GetOrDefault<MooiDocumentDto>();
            set => RaiseAndSetIfChanged(value);
        }
    }
}
