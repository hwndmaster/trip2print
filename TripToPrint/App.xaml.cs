using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Autofac;

using CefSharp;

using TripToPrint.Core;
using TripToPrint.Presenters;
using TripToPrint.Views;

namespace TripToPrint
{
    public partial class App
    {
        private IContainer _container;

        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            InitializeCefSharp();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _container = IocBootstrap.Init();

            var mainWindowPresenter = _container.Resolve<IMainWindowPresenter>();
            var mainWindowView = _container.Resolve<IMainWindowView>();
            mainWindowPresenter.InitializePresenter(mainWindowView);

            mainWindowView.Show();

            _container.Resolve<TestingEnv>().Run();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Cef.Shutdown();

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeCefSharp()
        {
            if (Cef.IsInitialized)
                return;

            var settings = new CefSettings {
                IgnoreCertificateErrors = true,
                LogSeverity = LogSeverity.Disable,
                //LogSeverity = LogSeverity.Default,
                //LogFile = "cef.log",
                BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    Environment.Is64BitProcess ? "x64" : "x86",
                    "CefSharp.BrowserSubprocess.exe")
            };
            CefSharpSettings.WcfTimeout = TimeSpan.Zero;

            Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
        }

        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);

                return File.Exists(archSpecificPath)
                           ? Assembly.LoadFile(archSpecificPath)
                           : null;
            }

            return null;
        }
    }
}
