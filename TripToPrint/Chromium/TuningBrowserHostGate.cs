using TripToPrint.ReportTuning.Dto;
using TripToPrint.Views;

namespace TripToPrint.Chromium
{
    public class TuningBrowserHostGate : IHostGate
    {
        ITuningBrowserView _view;

        public TuningBrowserHostGate(ITuningBrowserView view)
        {
            _view = view;
        }

        public void DocumentInitialized()
        {
            _view.RefreshData();
        }
    }
}
