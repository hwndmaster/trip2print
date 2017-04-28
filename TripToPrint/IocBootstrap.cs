using System.Diagnostics.CodeAnalysis;

using Autofac;

using TripToPrint.Core;
using TripToPrint.Presenters;
using TripToPrint.Services;
using TripToPrint.Views;

namespace TripToPrint
{
    [ExcludeFromCodeCoverage]
    public class IocBootstrap
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new IocModule());

            // User session
            builder.RegisterType<UserSession>().As<IUserSession>().SingleInstance();

            // Services
            builder.RegisterType<DialogService>().As<IDialogService>();

            // Presenters
            builder.RegisterType<MainWindowPresenter>().As<IMainWindowPresenter>()
                .InstancePerLifetimeScope();
            builder.RegisterType<StepIntroPresenter>().As<IStepIntroPresenter>();
            builder.RegisterType<StepSettingPresenter>().As<IStepSettingPresenter>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterType<StepGenerationPresenter>().As<IStepGenerationPresenter>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterType<KmlObjectsTreePresenter>().As<IKmlObjectsTreePresenter>();
            builder.RegisterType<StepAdjustmentPresenter>().As<IStepAdjustmentPresenter>();
            builder.RegisterType<AdjustBrowserViewPresenter>().As<IAdjustBrowserViewPresenter>();

            // Views
            builder.RegisterType<MainWindow>().As<IMainWindowView>();
            builder.RegisterType<StepIntro>().As<IStepIntroView>();
            builder.RegisterType<StepSettingView>().As<IStepSettingView>();
            builder.RegisterType<StepGenerationView>().As<IStepGenerationView>();
            builder.RegisterType<StepAdjustmentView>().As<IStepAdjustmentView>();
            builder.RegisterType<KmlObjectsTreeView>().As<IKmlObjectsTreeView>();
            builder.RegisterType<AdjustBrowserView>().As<IAdjustBrowserView>();

            // Misc
            builder.RegisterType<TestingEnv>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            return builder.Build();
        }
    }
}
