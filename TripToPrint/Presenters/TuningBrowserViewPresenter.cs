using System;
using System.Threading.Tasks;
using CefSharp;
using TripToPrint.Core.Logging;
using TripToPrint.ViewModels;
using TripToPrint.Views;
using TripToPrint.Chromium;
using TripToPrint.Core.Models;

namespace TripToPrint.Presenters
{
    public interface ITuningBrowserViewPresenter : IPresenter<TuningBrowserViewModel, ITuningBrowserView>, IDisposable
    {
        void HandleConsoleMessage(string message);
        ILogger GetLogger();
        Task<bool> SavePdfReportAsync(string path);
        void HandleActivated(string reportTempPath, MooiDocument document);
    }

    public class TuningBrowserViewPresenter : ITuningBrowserViewPresenter
    {
        private readonly ILogger _logger;
        private readonly ITuningDtoFactory _tuningDtoFactory;

        public TuningBrowserViewPresenter(ILogger logger, ITuningDtoFactory tuningDtoFactory)
        {
            _logger = logger;
            _tuningDtoFactory = tuningDtoFactory;
        }

        public ITuningBrowserView View { get; private set; }
        public virtual TuningBrowserViewModel ViewModel { get; private set; }

        public void InitializePresenter(ITuningBrowserView view, TuningBrowserViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new TuningBrowserViewModel();

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

        public void HandleActivated(string reportTempPath, MooiDocument document)
        {
            ViewModel.Document = _tuningDtoFactory.Create(document, reportTempPath);

            View.HandleActivated();
        }
    }
}
