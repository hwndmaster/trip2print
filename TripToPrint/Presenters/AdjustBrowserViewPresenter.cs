using System;
using System.Threading.Tasks;

using CefSharp;

using TripToPrint.Core.Logging;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IAdjustBrowserViewPresenter : IPresenter<AdjustBrowserViewModel, IAdjustBrowserView>, IDisposable
    {
        void HandleConsoleMessage(string message);
        ILogger GetLogger();
        Task<bool> SavePdfReportAsync(string path);
    }

    public class AdjustBrowserViewPresenter : IAdjustBrowserViewPresenter
    {
        private readonly ILogger _logger;

        public AdjustBrowserViewPresenter(ILogger logger)
        {
            _logger = logger;
        }

        public IAdjustBrowserView View { get; private set; }
        public virtual AdjustBrowserViewModel ViewModel { get; private set; }

        public void InitializePresenter(IAdjustBrowserView view, AdjustBrowserViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new AdjustBrowserViewModel();

            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            View.InitializeBrowser();
        }

        public void HandleConsoleMessage(string message)
        {
            _logger.Info(message);
        }

        public ILogger GetLogger()
        {
            return _logger;
        }

        public void Dispose()
        {
            View?.Dispose();
        }

        public async Task<bool> SavePdfReportAsync(string path)
        {
            return await View.Browser.PrintToPdfAsync(path, new PdfPrintSettings {
                MarginType = CefPdfPrintMarginType.Custom,
                MarginTop = 20,
                MarginBottom = 20,
                MarginLeft = 20,
                MarginRight = 20
            });
        }
    }
}
