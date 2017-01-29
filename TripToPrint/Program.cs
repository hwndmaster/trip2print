using System;
using System.Windows.Forms;

using Autofac;

using TripToPrint.Presenters;
using TripToPrint.Views;

namespace TripToPrint
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var container = IocBootstrap.Init();

            var mainFormPresenter = container.Resolve<IMainFormPresenter>();
            mainFormPresenter.InitializePresenter(container.Resolve<IMainFormView>());

            Application.Run(mainFormPresenter.View as Form);
        }
    }
}
