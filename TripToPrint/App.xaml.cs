using System;
using System.IO;
using System.Windows;
using Autofac;

using TripToPrint.Core;
using TripToPrint.Presenters;
using TripToPrint.Views;

namespace TripToPrint
{
    public partial class App
    {
        private IContainer _container;

        protected override void OnStartup(StartupEventArgs e)
        {
            _container = IocBootstrap.Init();

            var mainWindowPresenter = _container.Resolve<IMainWindowPresenter>();
            var mainWindowView = _container.Resolve<IMainWindowView>();
            mainWindowPresenter.InitializePresenter(mainWindowView);

            mainWindowView.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            CleanupTemporaryFiles();

            _container.Dispose();

            base.OnExit(e);
        }

        private void CleanupTemporaryFiles()
        {
            var resourceNameProvider = _container.Resolve<IResourceNameProvider>();

            var myTemporaryFolders = Directory.EnumerateDirectories(Path.GetTempPath(),
                resourceNameProvider.GetTempFolderPrefix() + "*", SearchOption.TopDirectoryOnly);
            try
            {
                foreach (var folderToDelete in myTemporaryFolders)
                    Directory.Delete(folderToDelete, true);
            }
            catch (Exception)
            {
            }
        }
    }
}
