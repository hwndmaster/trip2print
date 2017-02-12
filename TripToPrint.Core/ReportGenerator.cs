using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Codaxy.WkHtmlToPdf;

using TripToPrint.Core.Logging;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IReportGenerator
    {
        Task<string> Generate(string inputFileName, IProgressTracker progress);

        void SaveHtmlReportAsPdf(string tempPath, string pdfFilePath);
    }

    public class ReportGenerator : IReportGenerator
    {
        private readonly IKmlDocumentFactory _kmlDocumentFactory;
        private readonly IMooiDocumentFactory _mooiDocumentFactory;
        private readonly IHereAdapter _hereAdapter;
        private readonly IReportWriter _reportWriter;
        private readonly ILogger _logger;
        private readonly IFileService _file;
        private readonly IZipService _zipService;
        private readonly IResourceNameProvider _resourceName;
        private readonly IWebClientService _webClient;

        public ReportGenerator(IKmlDocumentFactory kmlDocumentFactory, IMooiDocumentFactory mooiDocumentFactory, IHereAdapter hereAdapter, IReportWriter reportWriter, ILogger logger, IFileService file, IZipService zipService, IResourceNameProvider resourceName, IWebClientService webClient)
        {
            _kmlDocumentFactory = kmlDocumentFactory;
            _mooiDocumentFactory = mooiDocumentFactory;
            _hereAdapter = hereAdapter;
            _reportWriter = reportWriter;
            _logger = logger;
            _file = file;
            _zipService = zipService;
            _resourceName = resourceName;
            _webClient = webClient;
        }

        public async Task<string> Generate(string inputFileName, IProgressTracker progress)
        {
            var ext = Path.GetExtension(inputFileName);

            if (ext.Equals(".kmz", StringComparison.OrdinalIgnoreCase))
            {
                return await GenerateForKmz(inputFileName, progress);
            }
            if (ext.Equals(".kml", StringComparison.OrdinalIgnoreCase))
            {
                return await GenerateForKml(inputFileName, progress);
            }

            throw new NotSupportedException();
        }

        public void SaveHtmlReportAsPdf(string tempPath, string pdfFilePath)
        {
            var environment = new PdfConvertEnvironment {
                WkHtmlToPdfPath = @"wkhtmltopdf.exe",
                Timeout = 60000
            };

            PdfConvert.ConvertHtmlToPdf(new PdfDocument {
                Url = Path.Combine(tempPath, _resourceName.GetDefaultHtmlReportName())
            }, environment, new PdfOutput { OutputFilePath = pdfFilePath });
        }

        public virtual async Task<string> GenerateForKml(string inputFileName, IProgressTracker progress)
        {
            throw new NotImplementedException("Sorry, KML files are not supported at the moment");
        }

        public virtual async Task<string> GenerateForKmz(string kmzFileName, IProgressTracker progress)
        {
            var tempPath = CreateAndGetTempPath();

            _logger.Info($"Reading '{kmzFileName}'");
            using (var zip = _zipService.Open(kmzFileName))
            {
                var resourceEntries = zip.GetFileNames().Where(x => x.StartsWith("images/")).ToList();
                foreach (var filename in resourceEntries)
                {
                    zip.SaveToFolder(filename, tempPath);
                }
                progress.ReportResourceEntriesProcessed();

                var kmlFileName = zip.GetFileNames().FirstOrDefault(x => Path.GetExtension(x).Equals(".kml"));
                if (kmlFileName == null)
                {
                    throw new InvalidOperationException("Provided KMZ file is invalid. An entry for KML was not found");
                }
                var kmlContent = await zip.GetFileContent(kmlFileName);

                var kmlDocument = _kmlDocumentFactory.Create(kmlContent);
                var mooiDocument = _mooiDocumentFactory.Create(kmlDocument);

                var content = await _reportWriter.WriteReportAsync(mooiDocument);

                progress.ReportContentGenerationDone();

                await FetchMapImages(mooiDocument, tempPath, progress);

                await _file.WriteStringAsync(Path.Combine(tempPath, _resourceName.GetDefaultHtmlReportName()), content);

                progress.ReportDone();
            }

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
                imageBytes = await _hereAdapter.FetchImage(placemark);
                filePath = Path.Combine(tempPath, _resourceName.CreateFileNameForPlacemarkThumbnail(placemark));
                await _file.WriteBytesAsync(filePath, imageBytes);
            }

            if (placemark.IconPathIsOnWeb)
            {
                filePath = Path.Combine(tempPath, StringHelper.MakeStringSafeForFileName(placemark.IconPath));
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
