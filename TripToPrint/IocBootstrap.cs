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
            builder.RegisterType<MainFormPresenter>().As<IMainFormPresenter>();
            builder.RegisterType<MainFormView>().As<IMainFormView>();

            return builder.Build();
        }
    }
}
