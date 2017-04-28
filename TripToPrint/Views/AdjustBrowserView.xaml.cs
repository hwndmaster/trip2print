using System;
using System.Diagnostics.CodeAnalysis;
using CefSharp;
using TripToPrint.Chromium;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IAdjustBrowserView : IView<IAdjustBrowserViewPresenter>, IDisposable
    {
        void InitializeBrowser();
    }

    [ExcludeFromCodeCoverage]
    public partial class AdjustBrowserView : IAdjustBrowserView
    {
        public AdjustBrowserView()
        {
            InitializeComponent();
        }

        public void InitializeBrowser()
        {
            browser.AllowDrop = false;
            browser.ConsoleMessage += Browser_ConsoleMessage;
            browser.BrowserSettings.ApplicationCache = CefState.Disabled;
            browser.RequestHandler = new RequestHandler(Presenter.GetLogger());
            browser.MenuHandler = new MenuHandler();
        }

        public IAdjustBrowserViewPresenter Presenter { get; set; }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Presenter.HandleConsoleMessage($"[{e.Source}:{e.Line}] {e.Message}");
        }

        public void Dispose()
        {
            browser?.Dispose();
        }
    }
}
