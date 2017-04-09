using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core.Logging;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.Tests.UnitTests
{
    [TestClass]
    public class ReportGeneratorTests
    {
        private readonly Mock<IMooiDocumentFactory> _mooiDocumentFactoryMock = new Mock<IMooiDocumentFactory>();
        private readonly Mock<IHereAdapter> _hereAdapterMock = new Mock<IHereAdapter>();
        private readonly Mock<IReportWriter> _reportWriterMock = new Mock<IReportWriter>();
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();
        private readonly Mock<IFileService> _fileMock = new Mock<IFileService>();
        private readonly Mock<IResourceNameProvider> _resourceNameMock = new Mock<IResourceNameProvider>();
        private readonly Mock<IWebClientService> _webClientMock = new Mock<IWebClientService>();
        private readonly Mock<IProgressTracker> _progressTrackerMock = new Mock<IProgressTracker>();

        private Mock<ReportGenerator> _reportGenerator;

        private string _tempFolderName = "temp-folder-name";

        [TestInitialize]
        public void TestInitialize()
        {
            _reportGenerator = new Mock<ReportGenerator>(
                _mooiDocumentFactoryMock.Object,
                _hereAdapterMock.Object,
                _reportWriterMock.Object,
                _loggerMock.Object,
                _fileMock.Object,
                _resourceNameMock.Object,
                _webClientMock.Object) {
                CallBase = true
            };

            _resourceNameMock.Setup(x => x.GetDefaultHtmlReportName()).Returns("default");
            _resourceNameMock.Setup(x => x.CreateTempFolderName(It.IsAny<string>())).Returns(_tempFolderName);

            _resourceNameMock.Setup(x => x.CreateFileNameForOverviewMap(It.IsAny<MooiGroup>())).Returns(string.Empty);
            _resourceNameMock.Setup(x => x.CreateFileNameForPlacemarkThumbnail(It.IsAny<MooiPlacemark>())).Returns(string.Empty);
        }

        [TestMethod]
        public async Task When_generating_report_the_report_is_created_and_path_is_returned()
        {
            // Arrange
            var document = new KmlDocument {
                Resources = {
                    new KmlResource { FileName = "resource-1", Blob = new byte[] {1}},
                    new KmlResource { FileName = "resource-2", Blob = new byte[] {2}}
                }
            };
            _reportGenerator.Setup(x => x.FetchMapImages(It.IsAny<MooiDocument>(), It.IsAny<string>(), _progressTrackerMock.Object))
                .Returns(Task.CompletedTask);
            _reportWriterMock.Setup(x => x.WriteReportAsync(It.IsAny<MooiDocument>())).Returns(Task.FromResult("content"));

            // Act
            var result = await _reportGenerator.Object.Generate(document, _progressTrackerMock.Object);

            // Verify
            Assert.IsTrue(result.StartsWith(Path.Combine(Path.GetTempPath(), _tempFolderName)));
            AssertDocumentResourcesAreSaved(document);
            _fileMock.Verify(x => x.WriteStringAsync(Path.Combine(result, "default"), "content"), Times.Once);
            _reportGenerator.Verify(x => x.FetchMapImages(It.IsAny<MooiDocument>(), result, _progressTrackerMock.Object), Times.Once);
        }

        [TestMethod]
        public async Task When_generating_report_for_kmz_the_progress_is_being_updated_properly()
        {
            // Arrange
            var document = new KmlDocument();
            _reportGenerator.Setup(x => x.FetchMapImages(It.IsAny<MooiDocument>(), It.IsAny<string>(), _progressTrackerMock.Object))
                .Returns(Task.CompletedTask);

            // Act
            await _reportGenerator.Object.Generate(document, _progressTrackerMock.Object);

            // Verify
            _progressTrackerMock.Verify(x => x.ReportResourceEntriesProcessed(), Times.Once);
            _progressTrackerMock.Verify(x => x.ReportContentGenerationDone(), Times.Once);
            _progressTrackerMock.Verify(x => x.ReportDone(), Times.Once);
        }

        [TestMethod]
        public async Task When_fetching_images_the_progress_is_being_updated_and_images_downloaded()
        {
            // Arrange
            var placemark = new MooiPlacemark();
            var group = new MooiGroup { Placemarks = { placemark } };
            var document = new MooiDocument { Sections = { new MooiSection { Groups = { group } } } };
            var tempPath = "temp-path";
            var overviewBytes = new byte[] { 0x22 };
            var thumbnailBytes = new byte[] { 0x44 };
            _resourceNameMock.Setup(x => x.CreateFileNameForOverviewMap(group)).Returns("overview-path");
            _resourceNameMock.Setup(x => x.CreateFileNameForPlacemarkThumbnail(placemark)).Returns("thumb-path");
            _hereAdapterMock.Setup(x => x.FetchOverviewMap(group))
                .Returns(Task.FromResult(overviewBytes));
            _hereAdapterMock.Setup(x => x.FetchThumbnail(placemark))
                .Returns(Task.FromResult(thumbnailBytes));

            // Act
            await _reportGenerator.Object.FetchMapImages(document, tempPath, _progressTrackerMock.Object);

            // Verify
            _fileMock.Verify(x => x.WriteBytesAsync(tempPath + @"\overview-path", overviewBytes));
            _fileMock.Verify(x => x.WriteBytesAsync(tempPath + @"\thumb-path", thumbnailBytes));
            _progressTrackerMock.Verify(x => x.ReportFetchImagesCount(1 + 1), Times.Once);
            _progressTrackerMock.Verify(x => x.ReportFetchImageProcessed(), Times.Exactly(2));
        }

        [TestMethod]
        public async Task When_fetching_placemark_thumbnails_with_icons_on_the_web_the_icons_are_downloaded()
        {
            // Arrange
            var placemark = new MooiPlacemark {
                IconPath = "http://icon-path"
            };
            var document = new MooiDocument {
                Sections = {
                    new MooiSection { Groups = { new MooiGroup { Placemarks = { placemark } } } }
                }
            };
            var tempPath = "temp-path";
            var iconBytes = new byte[] { 0x55 };
            _webClientMock.Setup(x => x.GetAsync(new Uri(placemark.IconPath))).Returns(Task.FromResult(iconBytes));

            // Act
            await _reportGenerator.Object.FetchMapImages(document, tempPath, _progressTrackerMock.Object);

            // Verify
            _fileMock.Verify(x => x.WriteBytesAsync(tempPath + @"\iconpath", iconBytes));
        }

        private void AssertDocumentResourcesAreSaved(KmlDocument document)
        {
            foreach (var entry in document.Resources)
            {
                _fileMock.Verify(x => x.WriteBytesAsync(It.Is<string>(s => s.Contains(entry.FileName)), entry.Blob));
            }
        }
    }
}
