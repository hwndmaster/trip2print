using Autofac;

using TripToPrint.Core;
using TripToPrint.Presenters;
using TripToPrint.Services;
using TripToPrint.Views;

namespace TripToPrint
{
    public class IocBootstrap
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new IocModule());

            // Services
            builder.RegisterType<DialogService>().As<IDialogService>();

            // Views and Presenters
            builder.RegisterType<MainWindowPresenter>().As<IMainWindowPresenter>();
            builder.RegisterType<StepIntroPresenter>().As<IStepIntroPresenter>();
            builder.RegisterType<StepGenerationPresenter>().As<IStepGenerationPresenter>();
            builder.RegisterType<StepAdjustmentPresenter>().As<IStepAdjustmentPresenter>();
            builder.RegisterType<MainWindow>().As<IMainWindowView>();
            builder.RegisterType<StepIntro>().As<IStepIntroView>();
            builder.RegisterType<StepGenerationView>().As<IStepGenerationView>();
            builder.RegisterType<StepAdjustmentView>().As<IStepAdjustmentView>();

            return builder.Build();
        }
    }
}
