using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TripToPrint.Core.Logging;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IReportGenerator
    {
        Task<string> Generate(KmlDocument document, Dictionary<KmlPlacemark, DiscoveredPlace> discoveredPlacePerPlacemark, IProgressTracker progress);
    }

    public class ReportGenerator : IReportGenerator
    {
        private readonly IMooiDocumentFactory _mooiDocumentFactory;
        private readonly IHereAdapter _hereAdapter;
        private readonly IReportWriter _reportWriter;
        private readonly ILogger _logger;
        private readonly IFileService _file;
        private readonly IResourceNameProvider _resourceName;
        private readonly IWebClientService _webClient;

        public ReportGenerator(IMooiDocumentFactory mooiDocumentFactory, IHereAdapter hereAdapter,
            IReportWriter reportWriter, ILogger logger, IFileService file, IResourceNameProvider resourceName,
            IWebClientService webClient)
        {
            _mooiDocumentFactory = mooiDocumentFactory;
            _hereAdapter = hereAdapter;
            _reportWriter = reportWriter;
            _logger = logger;
            _file = file;
            _resourceName = resourceName;
            _webClient = webClient;
        }

        public async Task<string> Generate(KmlDocument document, Dictionary<KmlPlacemark, DiscoveredPlace> discoveredPlacePerPlacemark, IProgressTracker progress)
        {
            var tempPath = CreateAndGetTempPath();

            foreach (var resource in document.Resources)
            {
                await _file.WriteBytesAsync(Path.Combine(tempPath, resource.FileName), resource.Blob);
            }
            progress.ReportResourceEntriesProcessed();

            var mooiDocument = _mooiDocumentFactory.Create(document, discoveredPlacePerPlacemark);
            var content = await _reportWriter.WriteReportAsync(mooiDocument);

            progress.ReportContentGenerationDone();

            await FetchMapImages(mooiDocument, tempPath, progress);

            await _file.WriteStringAsync(Path.Combine(tempPath, _resourceName.GetDefaultHtmlReportName()), content);
            progress.ReportDone();

            return tempPath;
        }

        public virtual async Task FetchMapImages(MooiDocument document, string tempPath, IProgressTracker progress)
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
            var filePath = Path.Combine(tempPath, _resourceName.CreateFileNameForOverviewMap(group));
            await _file.WriteBytesAsync(filePath, imageBytes);
            _logger.Info($"An overview map for '{group.Id}' has been successfully downloaded");
        }

        private async Task FetchPlacemarkMapImage(MooiPlacemark placemark, string tempPath)
        {
            string filePath;
            byte[] imageBytes;

            if (placemark.Type == PlacemarkType.Point)
            {
                imageBytes = await _hereAdapter.FetchThumbnail(placemark);
                filePath = Path.Combine(tempPath, _resourceName.CreateFileNameForPlacemarkThumbnail(placemark));
                await _file.WriteBytesAsync(filePath, imageBytes);
            }

            if (placemark.IconPathIsOnWeb)
            {
                filePath = Path.Combine(tempPath, StringHelper.MakeUrlStringSafeForFileName(placemark.IconPath));
                if (!_file.Exists(filePath))
                {
                    imageBytes = await _webClient.GetAsync(new Uri(placemark.IconPath));
                    await _file.WriteBytesAsync(filePath, imageBytes);
                }
            }

            _logger.Info($"A thumbnail image for '{placemark.Id}' has been successfully downloaded");
        }

        private string CreateAndGetTempPath()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), _resourceName.CreateTempFolderName());

            _file.CreateFolder(tempPath);

            return tempPath;
        }
    }
}
