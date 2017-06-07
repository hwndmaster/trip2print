using System.Diagnostics.CodeAnalysis;
using Autofac;
using TripToPrint.Core;
using TripToPrint.Presenters;
using TripToPrint.Services;
using TripToPrint.Views;

namespace TripToPrint
{
    [ExcludeFromCodeCoverage]
    public sealed class IocBootstrap
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new IocModule());

            // User session
            builder.RegisterType<UserSession>().As<IUserSession>().SingleInstance();

            // Services
            builder.RegisterType<ClipboardService>().As<IClipboardService>();
            builder.RegisterType<DialogService>().As<IDialogService>();
            builder.RegisterType<ProcessService>().As<IProcessService>();

            // Presenters
            builder.RegisterType<MainWindowPresenter>().As<IMainWindowPresenter>()
                .InstancePerLifetimeScope();
            builder.RegisterType<StepIntroPresenter>().As<IStepIntroPresenter>();
            builder.RegisterType<StepPickPresenter>().As<IStepPickPresenter>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterType<StepDiscoveringPresenter>().As<IStepDiscoveringPresenter>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterType<StepExplorePresenter>().As<IStepExplorePresenter>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterType<StepGenerationPresenter>().As<IStepGenerationPresenter>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterType<StepTuningPresenter>().As<IStepTuningPresenter>();
            builder.RegisterType<KmlObjectsTreePresenter>().As<IKmlObjectsTreePresenter>();
            builder.RegisterType<TuningBrowserViewPresenter>().As<ITuningBrowserViewPresenter>();

            // Views
            builder.RegisterType<MainWindow>().As<IMainWindowView>();
            builder.RegisterType<StepIntro>().As<IStepIntroView>();
            builder.RegisterType<StepPickView>().As<IStepPickView>();
            builder.RegisterType<StepExploreView>().As<IStepExploreView>();
            builder.RegisterType<StepInProgressView>().As<IStepInProgressView>();
            builder.RegisterType<StepTuningView>().As<IStepTuningView>();
            builder.RegisterType<KmlObjectsTreeView>().As<IKmlObjectsTreeView>();
            builder.RegisterType<TuningBrowserView>().As<ITuningBrowserView>();

            // Misc
            builder.RegisterType<TuningDtoFactory>().As<ITuningDtoFactory>();

            // For debugging purposes
#if DEBUG
            builder.RegisterType<TestingEnv>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
#endif

            return builder.Build();
        }
    }
}
