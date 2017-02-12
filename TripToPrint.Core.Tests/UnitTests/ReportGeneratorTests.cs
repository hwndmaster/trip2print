using System;
using System.IO;
using System.Linq;
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
        private readonly Mock<IKmlDocumentFactory> _kmlDocumentFactoryMock = new Mock<IKmlDocumentFactory>();
        private readonly Mock<IMooiDocumentFactory> _mooiDocumentFactoryMock = new Mock<IMooiDocumentFactory>();
        private readonly Mock<IHereAdapter> _hereAdapterMock = new Mock<IHereAdapter>();
        private readonly Mock<IReportWriter> _reportWriterMock = new Mock<IReportWriter>();
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();
        private readonly Mock<IFileService> _fileMock = new Mock<IFileService>();
        private readonly Mock<IZipService> _zipServiceMock = new Mock<IZipService>();
        private readonly Mock<IResourceNameProvider> _resourceNameMock = new Mock<IResourceNameProvider>();
        private readonly Mock<IWebClientService> _webClientMock = new Mock<IWebClientService>();

        private readonly Mock<IProgressTracker> _progressTrackerMock = new Mock<IProgressTracker>();

        private Mock<ReportGenerator> _reportGenerator;

        private string[] _zipFileEntries = {
            "images/resource-1",
            "images/resource-2",
            "file.kml"
        };

        private string _tempFolderName = "temp-folder-name";

        [TestInitialize]
        public void TestInitialize()
        {
            _reportGenerator = new Mock<ReportGenerator>(
                _kmlDocumentFactoryMock.Object,
                _mooiDocumentFactoryMock.Object,
                _hereAdapterMock.Object,
                _reportWriterMock.Object,
                _loggerMock.Object,
                _fileMock.Object,
                _zipServiceMock.Object,
                _resourceNameMock.Object,
                _webClientMock.Object) {
                CallBase = true
            };

            _resourceNameMock.Setup(x => x.GetDefaultHtmlReportName()).Returns("default");
            _resourceNameMock.Setup(x => x.CreateTempFolderName(It.IsAny<string>())).Returns(_tempFolderName);
        }

        [TestMethod]
        public void When_generating_report_with_filetype_of_kmz_a_proper_handler_is_invoked()
        {
            // Arrange
            _reportGenerator.Setup(x => x.GenerateForKmz("input-name.kmz", _progressTrackerMock.Object))
                .Returns(Task.FromResult("test-passed"));

            // Act
            var result = _reportGenerator.Object.Generate("input-name.kmz", _progressTrackerMock.Object).GetAwaiter().GetResult();

            // Verify
            Assert.AreEqual("test-passed", result);
        }

        [TestMethod]
        public void When_generating_report_with_filetype_of_kml_a_proper_handler_is_invoked()
        {
            // Arrange
            _reportGenerator.Setup(x => x.GenerateForKml("input-name.kml", _progressTrackerMock.Object))
                .Returns(Task.FromResult("test-passed"));

            // Act
            var result = _reportGenerator.Object.Generate("input-name.kml", _progressTrackerMock.Object).GetAwaiter().GetResult();

            // Verify
            Assert.AreEqual("test-passed", result);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void When_generating_report_for_unknown_filetype_an_exception_is_thrown()
        {
            _reportGenerator.Object.Generate("input-name.kkk", _progressTrackerMock.Object).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void When_generating_report_for_kmz_the_report_is_created_and_path_is_returned()
        {
            // Arrange
            var zipFileMock = CreateSampleZipFile("zip-file-name");
            _reportGenerator.Setup(x => x.FetchMapImages(It.IsAny<MooiDocument>(), It.IsAny<string>(), _progressTrackerMock.Object))
                .Returns(Task.CompletedTask);
            _reportWriterMock.Setup(x => x.WriteReportAsync(It.IsAny<MooiDocument>())).Returns(Task.FromResult("content"));

            // Act
            var result = _reportGenerator.Object.GenerateForKmz("zip-file-name", _progressTrackerMock.Object).GetAwaiter().GetResult();

            // Verify
            Assert.IsTrue(result.StartsWith(Path.Combine(Path.GetTempPath(), _tempFolderName)));
            AssertZipFileResourcesAreSaved(zipFileMock, _zipFileEntries);
            _fileMock.Verify(x => x.WriteStringAsync(Path.Combine(result, "default"), "content"), Times.Once);
            _reportGenerator.Verify(x => x.FetchMapImages(It.IsAny<MooiDocument>(), result, _progressTrackerMock.Object), Times.Once);
        }

        [TestMethod]
        public void When_generating_report_for_kmz_the_progress_is_being_updated_properly()
        {
            // Arrange
            CreateSampleZipFile("zip-file-name");
            _reportGenerator.Setup(x => x.FetchMapImages(It.IsAny<MooiDocument>(), It.IsAny<string>(), _progressTrackerMock.Object))
                .Returns(Task.CompletedTask);

            // Act
            _reportGenerator.Object.GenerateForKmz("zip-file-name", _progressTrackerMock.Object).GetAwaiter().GetResult();

            // Verify
            _progressTrackerMock.Verify(x => x.ReportResourceEntriesProcessed(), Times.Once);
            _progressTrackerMock.Verify(x => x.ReportContentGenerationDone(), Times.Once);
            _progressTrackerMock.Verify(x => x.ReportDone(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_generating_report_for_file_without_kml_file_inside_an_exception_is_thrown()
        {
            // Arrange
            CreateSampleZipFile("zip-file-name", new [] { "another-file-in-zip" });

            // Act
            _reportGenerator.Object.GenerateForKmz("zip-file-name", _progressTrackerMock.Object).GetAwaiter().GetResult();
        }

        private Mock<IZipFileWrapper> CreateSampleZipFile(string zipFilename, string[] entries = null)
        {
            var mock = new Mock<IZipFileWrapper>();

            mock.Setup(x => x.GetFileNames()).Returns(entries ?? _zipFileEntries);

            _zipServiceMock.Setup(x => x.Open(zipFilename)).Returns(mock.Object);

            return mock;
        }

        private void AssertZipFileResourcesAreSaved(Mock<IZipFileWrapper> zipFileMock, string[] entries)
        {
            var path = Path.Combine(Path.GetTempPath(), _tempFolderName);

            foreach (var entry in entries.Where(x => x.StartsWith("images/")))
            {
                zipFileMock.Verify(x => x.SaveToFolder(entry, path));
            }
        }
    }
}
