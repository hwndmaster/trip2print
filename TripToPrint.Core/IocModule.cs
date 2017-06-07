using System.Diagnostics.CodeAnalysis;
using Autofac;

using TripToPrint.Core.Integration;
using TripToPrint.Core.Logging;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.ProgressTracking;

namespace TripToPrint.Core
{
    [ExcludeFromCodeCoverage]
    public class IocModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Services
            builder.RegisterType<HereAdapter>().As<IHereAdapter>();
            builder.RegisterType<FoursquareAdapter>().As<IFoursquareAdapter>();
            builder.RegisterType<GoogleMyMapAdapter>().As<IGoogleMyMapAdapter>();
            builder.RegisterType<ReportResourceFetcher>().As<IReportResourceFetcher>();
            builder.RegisterType<KmlFileReader>().As<IKmlFileReader>();
            builder.RegisterType<FileService>().As<IFileService>();
            builder.RegisterType<ZipService>().As<IZipService>();
            builder.RegisterType<ResourceNameProvider>().As<IResourceNameProvider>();
            builder.RegisterType<WebClientService>().As<IWebClientService>();
            builder.RegisterType<KmlCalculator>().As<IKmlCalculator>();
            builder.RegisterType<DiscoveringService>().As<IDiscoveringService>();

            // Logging
            builder.RegisterType<LogStorage>().As<ILogStorage>().SingleInstance();
            builder.RegisterType<GeneralLogger>().As<ILogger>();
            builder.RegisterType<DiscoveringLogger>().As<IDiscoveringLogger>();
            builder.RegisterType<ResourceFetchingLogger>().As<IResourceFetchingLogger>();

            // Class Factories
            builder.RegisterType<ProgressTrackerFactory>().As<IProgressTrackerFactory>();

            // Model Factories
            builder.RegisterType<KmlDocumentFactory>().As<IKmlDocumentFactory>();
            builder.RegisterType<MooiDocumentFactory>().As<IMooiDocumentFactory>();
            builder.RegisterType<MooiGroupFactory>().As<IMooiGroupFactory>();
            builder.RegisterType<MooiPlacemarkFactory>().As<IMooiPlacemarkFactory>();

            base.Load(builder);
        }
    }
}
