using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using TripToPrint.Core.Logging;
using TripToPrint.Presenters;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Tests
{
    [TestClass]
    public class StepInProgressPresenterBaseTests
    {
        private readonly Mock<IStepInProgressView> _viewMock = new Mock<IStepInProgressView>();
        private readonly Mock<ILogStorage> _logStorageMock = new Mock<ILogStorage>();
        private readonly Mock<IResourceFetchingLogger> _loggerMock = new Mock<IResourceFetchingLogger>();

        private Mock<StepInProgressPresenterBase> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<StepInProgressPresenterBase>(
                _loggerMock.Object,
                _logStorageMock.Object)
            {
                CallBase = true
            };
        }

        [TestMethod]
        public void When_initializing_presenter_the_properties_are_set_correctly()
        {
            // Act
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Verify
            Assert.AreEqual(_viewMock.Object, _presenter.Object.View);
            Assert.IsNotNull(_presenter.Object.ViewModel);
            _viewMock.VerifySet(x => x.Presenter = _presenter.Object);
            _viewMock.VerifySet(x => x.DataContext = _presenter.Object.ViewModel);
        }

        [TestMethod]
        public async Task When_process_is_done_the_validation_to_go_next_is_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepInProgressViewModel { ProgressInPercentage = 100 });

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task When_process_is_not_done_the_validation_to_go_next_is_not_passing()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepInProgressViewModel { ProgressInPercentage = 99 });

            // Act
            var result = await _presenter.Object.BeforeGoNext();

            // Verify
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task When_going_back_with_unfinished_progress_the_token_is_cancelled()
        {
            // Arrange
            _presenter.SetupGet(x => x.ViewModel).Returns(new StepInProgressViewModel { ProgressInPercentage = 99 });

            // Act
            await _presenter.Object.BeforeGoBack();

            // Verify
            _presenter.Protected().Verify("CancelOperation", Times.Once());
        }

        [TestMethod]
        public void When_log_message_is_coming_up_the_view_is_notified()
        {
            // Arrange
            var logItem = new LogItem(LogCategory.ResourceFetching, LogSeverity.Info, "test");
            _loggerMock.SetupGet(x => x.Category).Returns(LogCategory.ResourceFetching);
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Act
            _logStorageMock.Raise(x => x.ItemAdded += null, null, logItem);

            // Verify
            _viewMock.Verify(x => x.AddLogItem(logItem));
        }

        [TestMethod]
        public void When_log_message_of_improper_category_is_coming_up_the_message_is_ignored()
        {
            // Arrange
            var logItem = new LogItem(LogCategory.General, LogSeverity.Info, "test");
            _loggerMock.SetupGet(x => x.Category).Returns(LogCategory.ResourceFetching);
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Act
            _logStorageMock.Raise(x => x.ItemAdded += null, null, logItem);

            // Verify
            _viewMock.Verify(x => x.AddLogItem(logItem), Times.Never);
        }

        [TestMethod]
        public void When_log_category_is_cleaned_up_the_view_is_notified()
        {
            // Arrange
            _loggerMock.SetupGet(x => x.Category).Returns(LogCategory.ResourceFetching);
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Act
            _logStorageMock.Raise(x => x.CategoryItemsRemoved += null, null, LogCategory.ResourceFetching);

            // Verify
            _viewMock.Verify(x => x.ClearLogItems());
        }

        [TestMethod]
        public void When_improper_log_category_is_cleaned_up_the_message_is_ignored()
        {
            // Arrange
            _loggerMock.SetupGet(x => x.Category).Returns(LogCategory.ResourceFetching);
            _presenter.Object.InitializePresenter(_viewMock.Object);

            // Act
            _logStorageMock.Raise(x => x.CategoryItemsRemoved += null, null, LogCategory.General);

            // Verify
            _viewMock.Verify(x => x.ClearLogItems(), Times.Never);
        }
    }
}
