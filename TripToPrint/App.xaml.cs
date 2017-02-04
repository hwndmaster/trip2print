using System.Windows;

using Autofac;

using TripToPrint.Presenters;
using TripToPrint.Views;

namespace TripToPrint
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var container = IocBootstrap.Init();

            var mainWindowPresenter = container.Resolve<IMainWindowPresenter>();
            var mainWindowView = container.Resolve<IMainWindowView>();
            mainWindowPresenter.InitializePresenter(mainWindowView);

            mainWindowView.Show();

            base.OnStartup(e);
        }
    }
}
