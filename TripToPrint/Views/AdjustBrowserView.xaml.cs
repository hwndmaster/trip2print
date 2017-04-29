using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

using CefSharp;
using CefSharp.Wpf;

using TripToPrint.Chromium;
using TripToPrint.Presenters;
using TripToPrint.ViewModels;

namespace TripToPrint.Views
{
    public interface IAdjustBrowserView : IView<IAdjustBrowserViewPresenter>, IDisposable
    {
        IWebBrowser Browser { get; }

        void InitializeBrowser();
        void HandleActivated();
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
            RecreateBrowser();

            var requestContextSettings = new RequestContextSettings { CachePath = "" };

            browser.AllowDrop = false;
            browser.ConsoleMessage += Browser_ConsoleMessage;
            browser.MenuHandler = new MenuHandler();
            browser.BrowserSettings = new BrowserSettings {
                ApplicationCache = CefState.Disabled
            };
            browser.RequestContext = new RequestContext(requestContextSettings);
            browser.RequestHandler = new RequestHandler(Presenter.GetLogger());
        }

        public void HandleActivated()
        {
            RecreateBrowser();
            InitializeBrowser();
        }

        public void Dispose()
        {
            browser?.Dispose();
        }

        private void Browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Presenter.HandleConsoleMessage($"[{e.Source}:{e.Line}] {e.Message}");
        }

        /// <summary>
        /// This is a temporary solution to CefSharp's resizing issue when a custom DPI is set to the OS.
        /// For more information: https://github.com/cefsharp/CefSharp/issues/1571
        /// </summary>
        private void RecreateBrowser()
        {
            var grid = browser.Parent as Grid;
            grid.Children.Remove(browser);
            browser.Dispose();

            browser = new ChromiumWebBrowser
            {
                Address = (DataContext as AdjustBrowserViewModel).Address
            };

            grid.Children.Add(browser);
        }
    }
}
