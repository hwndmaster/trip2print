using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Ionic.Zip;

using TripToPrint.Core.Logging;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IReportGenerator
    {
        Task Generate(string inputFileName, string reportFileName, IProgressTracker progress);
    }

    public class ReportGenerator : IReportGenerator
    {
        private readonly IKmlDocumentFactory _kmlDocumentFactory;
        private readonly IMooiDocumentFactory _mooiDocumentFactory;
        private readonly IHereAdapter _hereAdapter;
        private readonly IReportWriter _reportWriter;
        private readonly ILogger _logger;

        public ReportGenerator(IKmlDocumentFactory kmlDocumentFactory, IMooiDocumentFactory mooiDocumentFactory, IHereAdapter hereAdapter, IReportWriter reportWriter, ILogger logger)
        {
            _kmlDocumentFactory = kmlDocumentFactory;
            _mooiDocumentFactory = mooiDocumentFactory;
            _hereAdapter = hereAdapter;
            _reportWriter = reportWriter;
            _logger = logger;
        }

        public async Task Generate(string inputFileName, string reportFileName, IProgressTracker progress)
        {
            var ext = Path.GetExtension(inputFileName);

            if (ext.Equals(".kmz", StringComparison.OrdinalIgnoreCase))
            {
                await GenerateForKmz(inputFileName, reportFileName, progress);
            }
            else if (ext.Equals(".kml", StringComparison.OrdinalIgnoreCase))
            {
                await GenerateForKml(inputFileName, reportFileName, progress);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private async Task GenerateForKml(string inputFileName, string reportFileName, IProgressTracker progress)
        {
            throw new NotImplementedException("Sorry, KML files are not supported at the moment");
        }

        private async Task GenerateForKmz(string kmzFileName, string reportFileName, IProgressTracker progress)
        {
            _logger.Info($"Reading '{kmzFileName}'");
            using (var zip = ZipFile.Read(kmzFileName))
            {
                var kmlEntry = zip.Entries.First(x => Path.GetExtension(x.FileName).Equals(".kml"));
                using (var stream = new MemoryStream(new byte[kmlEntry.UncompressedSize]))
                {
                    kmlEntry.Extract(stream);

                    stream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(stream))
                    {
                        var resourcesPath = Path.Combine(Path.GetDirectoryName(reportFileName),
                            Path.GetFileNameWithoutExtension(reportFileName) + "_resources");

                        var resourceEntries = zip.Entries.Where(x => x.FileName.StartsWith("images/")).ToList();
                        foreach (var entry in resourceEntries)
                        {
                            entry.Extract(resourcesPath, ExtractExistingFileAction.OverwriteSilently);
                        }
                        progress.ReportResourceEntriesProcessed();

                        var kmlContent = await reader.ReadToEndAsync();
                        var kmlDocument = _kmlDocumentFactory.Create(kmlContent);
                        var mooiDocument = _mooiDocumentFactory.Create(kmlDocument);

                        var content = await _reportWriter.WriteReportAsync(mooiDocument, resourcesPath);

                        progress.ReportContentGenerationDone();

                        await FetchMapImages(mooiDocument, resourcesPath, progress);
                        await CopyGeneralResources(resourcesPath);

                        _logger.Info($"Writing report to '{reportFileName}'");
                        using (var reportStream = File.OpenWrite(reportFileName))
                        {
                            using (var reportWriter = new StreamWriter(reportStream))
                            {
                                await reportWriter.WriteAsync(content);
                            }
                        }

                        progress.ReportDone();
                    }
                }
            }
        }

        private async Task CopyGeneralResources(string resourcesPath)
        {
            //File.WriteAllText($"{resourcesPath}/jquery.min.js", Resources.JqueryScript);
            //File.WriteAllText($"{resourcesPath}/jquery.masonry.min.js", Resources.JqueryMasonryScript);
        }

        private async Task FetchMapImages(MooiDocument document, string resourcesPath, IProgressTracker progress)
        {
            var groups = document.Sections.SelectMany(x => x.Groups).ToList();
            var placemarks = document.Sections.SelectMany(x => x.Groups).SelectMany(x => x.Placemarks).ToList();

            progress.ReportFetchImagesCount(groups.Count + placemarks.Count);

            _logger.Info("Downloading overviews");
            foreach (var group in groups)
            {
                await FetchGroupMapImage(resourcesPath, group);

                progress.ReportFetchImageProcessed();
            }

            _logger.Info("Downloading sections");
            foreach (var placemark in placemarks)
            {
                await FetchPlacemarkMapImage(resourcesPath, placemark);

                progress.ReportFetchImageProcessed();
            }
        }

        private async Task FetchGroupMapImage(string resourcesPath, MooiGroup group)
        {
            var fileName = $"{resourcesPath}/{group.OverviewMapFileName}";
            if (File.Exists(fileName))
            {
                _logger.Warn($"A map image '{fileName}' wasn't downloaded since file already exists");
            }
            else
            {
                var imageBytes = await _hereAdapter.FetchOverviewMap(group);
                using (var stream = File.OpenWrite(fileName))
                {
                    await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                }

                _logger.Info($"A map image '{fileName}' has been successfully downloaded");
            }
        }

        private async Task FetchPlacemarkMapImage(string resourcesPath, MooiPlacemark placemark)
        {
            var fileName = $"{resourcesPath}/{placemark.Id}.jpg";
            if (File.Exists(fileName))
            {
                _logger.Warn($"A map image '{fileName}' wasn't downloaded since file already exists");
            }
            else
            {
                var imageBytes = await _hereAdapter.FetchImage(placemark);
                using (var stream = File.OpenWrite(fileName))
                {
                    await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                }
                _logger.Info($"A map image '{fileName}' has been successfully downloaded");
            }
        }
    }
}
