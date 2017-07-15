using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using TripToPrint.Chromium;
using TripToPrint.Presenters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TripToPrint.Views
{
    public interface ITuningBrowserView : IView<ITuningBrowserViewPresenter>, IDisposable
    {
        IWebBrowser Browser { get; }

        void InitializeBrowser();
        void HandleActivated();
        void RefreshData();
    }

    [ExcludeFromCodeCoverage]
    public sealed partial class TuningBrowserView : ITuningBrowserView
    {
        public TuningBrowserView()
        {
            InitializeComponent();
        }

        public ITuningBrowserViewPresenter Presenter { get; set; }
        public IWebBrowser Browser => browser;

        public void InitializeBrowser()
        {
            RecreateBrowser();

            browser.AllowDrop = false;
            browser.ConsoleMessage += Browser_ConsoleMessage;
            browser.MenuHandler = new MenuHandler();
            browser.BrowserSettings = new BrowserSettings {
                ApplicationCache = CefState.Disabled
            };
            browser.RequestHandler = new RequestHandler(Presenter.GetLogger());

            browser.RegisterJsObject("host", new TuningBrowserHostGate(this));
        }

        public void HandleActivated()
        {
            RecreateBrowser();
            InitializeBrowser();
        }

        public void RefreshData()
        {
            var data = JsonConvert.SerializeObject(Presenter.ViewModel.Document,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            browser.ExecuteScriptAsync($"app.applyData({data})");
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
                Address = "t2p://internal/index.html"
            };

            grid.Children.Add(browser);
        }
    }
}
