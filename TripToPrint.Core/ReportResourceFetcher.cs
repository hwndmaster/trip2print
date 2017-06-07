using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

using TripToPrint.Core.Integration;
using TripToPrint.Core.Logging;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;
using TripToPrint.Core.ProgressTracking;

namespace TripToPrint.Core
{
    public interface IReportResourceFetcher
    {
        Task<(string tempPath, MooiDocument document)> Generate(KmlDocument document, List<DiscoveredPlace> discoveredPlaces, string language, IResourceFetchingProgress progress);
    }

    internal class ReportResourceFetcher : IReportResourceFetcher
    {
        private readonly IMooiDocumentFactory _mooiDocumentFactory;
        private readonly IHereAdapter _hereAdapter;
        private readonly IFoursquareAdapter _foursquare;
        private readonly IResourceFetchingLogger _logger;
        private readonly IFileService _file;
        private readonly IResourceNameProvider _resourceName;

        public ReportResourceFetcher(IMooiDocumentFactory mooiDocumentFactory, IHereAdapter hereAdapter,
            IFoursquareAdapter foursquare, IResourceFetchingLogger logger, IFileService file, IResourceNameProvider resourceName)
        {
            _mooiDocumentFactory = mooiDocumentFactory;
            _hereAdapter = hereAdapter;
            _foursquare = foursquare;
            _logger = logger;
            _file = file;
            _resourceName = resourceName;
        }

        public async Task<(string tempPath, MooiDocument document)> Generate([NotNull] KmlDocument document
            , [NotNull] List<DiscoveredPlace> discoveredPlaces, string language, IResourceFetchingProgress progress)
        {
            // TODO: Add cancellationtoken approach

            var tempPath = CreateAndGetTempPath();

            foreach (var resource in document.Resources)
            {
                await _file.WriteBytesAsync(Path.Combine(tempPath, resource.FileName), resource.Blob);
            }
            progress.ReportResourceEntriesProcessed();

            await _foursquare.PopulateWithDetailedInfo(discoveredPlaces.Where(x => !x.IsForPlacemark), language, CancellationToken.None);

            var mooiDocument = _mooiDocumentFactory.Create(document, discoveredPlaces, tempPath);

            await FetchMapImages(mooiDocument, tempPath, progress);

            progress.ReportDone();

            return (tempPath, mooiDocument);
        }

        public virtual async Task FetchMapImages(MooiDocument document, string tempPath, IResourceFetchingProgress progress)
        {
            var groups = document.Sections.SelectMany(x => x.Groups).ToList();
            var placemarks = document.Sections.SelectMany(x => x.Groups).SelectMany(x => x.Placemarks).ToList();

            progress.ReportFetchImagesCount(groups.Count + placemarks.Count);

            _logger.Info("Downloading overviews");
            foreach (var group in groups)
            {
                await FetchGroupMapImage(group, tempPath);
                progress.ReportFetchImageProcessed();
            }

            _logger.Info("Downloading sections");
            foreach (var placemark in placemarks)
            {
                await FetchPlacemarkMapImage(placemark, tempPath);
                progress.ReportFetchImageProcessed();
            }
        }

        private async Task FetchGroupMapImage(MooiGroup group, string tempPath)
        {
            var imageBytes = await _hereAdapter.FetchOverviewMap(group);
            if (imageBytes == null)
            {
                _logger.Warn($"Was unable to download overview map image for '{group.Id}'");
                return;
            }
            var filePath = Path.Combine(tempPath, _resourceName.CreateFileNameForOverviewMap(group));
            await _file.WriteBytesAsync(filePath, imageBytes);
            _logger.Info($"An overview map image for '{group.Id}' has been successfully downloaded");
        }

        private async Task FetchPlacemarkMapImage(MooiPlacemark placemark, string tempPath)
        {
            if (placemark.Type == PlacemarkType.Point)
            {
                var imageBytes = await _hereAdapter.FetchThumbnail(placemark);
                if (imageBytes == null)
                {
                    _logger.Warn($"Was unable to download thumbnail map image for '{placemark.Id}'");
                    return;
                }
                var filePath = Path.Combine(tempPath, _resourceName.CreateFileNameForPlacemarkThumbnail(placemark));
                await _file.WriteBytesAsync(filePath, imageBytes);

                _logger.Info($"A thumbnail map image for '{placemark.Id}' has been successfully downloaded");
            }
        }

        private string CreateAndGetTempPath()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), _resourceName.CreateTempFolderName());

            _file.CreateFolder(tempPath);

            return tempPath;
        }
    }
}
