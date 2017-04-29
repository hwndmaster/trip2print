using System;
using System.Diagnostics.CodeAnalysis;
using CefSharp;
using TripToPrint.Chromium;
using TripToPrint.Presenters;

namespace TripToPrint.Views
{
    public interface IAdjustBrowserView : IView<IAdjustBrowserViewPresenter>, IDisposable
    {
        IWebBrowser Browser { get; }

        void InitializeBrowser();
    }

    [ExcludeFromCodeCoverage]
    public partial class AdjustBrowserView : IAdjustBrowserView
    {
        public AdjustBrowserView()
        {
            InitializeComponent();
        }

        public IAdjustBrowserViewPresenter Presenter { get; set; }
        public IWebBrowser Browser => browser;

        public void InitializeBrowser()
        {
            browser.AllowDrop = false;
            browser.ConsoleMessage += Browser_ConsoleMessage;
            browser.BrowserSettings.ApplicationCache = CefState.Disabled;
            browser.RequestHandler = new RequestHandler(Presenter.GetLogger());
            browser.MenuHandler = new MenuHandler();
        }

        public void Dispose()
        {
            browser?.Dispose();
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Presenter.HandleConsoleMessage($"[{e.Source}:{e.Line}] {e.Message}");
        }
    }
}
