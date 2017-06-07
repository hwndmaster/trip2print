using System.Diagnostics.CodeAnalysis;

using TripToPrint.ReportTuning.Dto;
using TripToPrint.Views;

namespace TripToPrint.Chromium
{
    [ExcludeFromCodeCoverage]
    public sealed class TuningBrowserHostGate : IHostGate
    {
        private readonly ITuningBrowserView _view;

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
