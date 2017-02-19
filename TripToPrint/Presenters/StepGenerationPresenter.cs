using System;
using System.IO;
using System.Threading.Tasks;

using TripToPrint.Core;
using TripToPrint.Core.Logging;
using TripToPrint.ViewModels;
using TripToPrint.Views;

namespace TripToPrint.Presenters
{
    public interface IStepGenerationPresenter : IPresenter<StepGenerationViewModel, IStepGenerationView>, IStepPresenter
    {
    }

    public class StepGenerationPresenter : IStepGenerationPresenter
    {
        private readonly IGoogleMyMapAdapter _googleMyMapAdapter;
        private readonly IReportGenerator _reportGenerator;
        private readonly ILogStorage _logStorage;
        private readonly ILogger _logger;
        private readonly IFileService _file;
        private readonly IWebClientService _webClient;
        private readonly IResourceNameProvider _resourceName;
        private readonly IProgressTrackerFactory _progressTrackerFactory;

        private const int PROGRESS_DONE_PERCENTAGE = 100;

        public StepGenerationPresenter(IReportGenerator reportGenerator, ILogStorage logStorage, ILogger logger, IWebClientService webClient, IFileService file, IGoogleMyMapAdapter googleMyMapAdapter, IResourceNameProvider resourceName, IProgressTrackerFactory progressTrackerFactory)
        {
            _googleMyMapAdapter = googleMyMapAdapter;
            _resourceName = resourceName;
            _progressTrackerFactory = progressTrackerFactory;
            _reportGenerator = reportGenerator;
            _logStorage = logStorage;
            _logger = logger;
            _file = file;
            _webClient = webClient;
        }


        public IStepGenerationView View { get; private set; }
        public virtual StepGenerationViewModel ViewModel { get; private set; }
        public event EventHandler GoNextRequested;

        public void InitializePresenter(IStepGenerationView view, StepGenerationViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new StepGenerationViewModel();
            View = view;
            View.DataContext = ViewModel;
            View.Presenter = this;

            _logStorage.ItemAdded += (sender, item) => View.AddLogItem(item);
            _logStorage.AllItemsRemoved += (sender, args) => View.ClearLogItems();
        }

        public async Task Activated()
        {
            await GenerateReport();
        }

        public bool BeforeToGoBack()
        {
            if (ViewModel.ProgressInPercentage < PROGRESS_DONE_PERCENTAGE)
            {
                // TODO: Break the process
            }

            return true;
        }

        public Task<bool> BeforeGoNext()
        {
            if (ViewModel.ProgressInPercentage < PROGRESS_DONE_PERCENTAGE)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public void GetBackNextTitles(ref string back, ref string next)
        {
        }

        private async Task GenerateReport()
        {
            string inputFileName = null;

            _logStorage.ClearAll();
            ViewModel.ProgressInPercentage = 0;

            try
            {
                var progressTracker = _progressTrackerFactory.Create(value => ViewModel.ProgressInPercentage = value);

                switch (ViewModel.InputSource)
                {
                    case InputSource.LocalFile:
                        inputFileName = ViewModel.InputUri;
                        break;
                    case InputSource.GoogleMyMapsUrl:
                        var errorMessage = "Cannot download KML file for provided URL. Make sure the map is shared publicly.";
                        var uri = _googleMyMapAdapter.GetKmlDownloadUrl(new Uri(ViewModel.InputUri));
                        if (uri == null)
                        {
                            throw new InvalidOperationException(errorMessage);
                        }
                        try
                        {
                            var inputData = await _webClient.GetAsync(uri);
                            inputFileName = $"{Path.GetTempPath()}{_resourceName.GetTempFolderPrefix()}{Guid.NewGuid()}.kmz";
                            await _file.WriteBytesAsync(inputFileName, inputData);
                        }
                        catch (Exception)
                        {
                            throw new InvalidOperationException(errorMessage);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Input source type is invalid: {ViewModel.InputSource}");
                }

                progressTracker.ReportSourceDownloaded();

                ViewModel.TempPath = await _reportGenerator.Generate(inputFileName, progressTracker);

                _logger.Info("Generation process complete");

                GoNextRequested?.Invoke(this, null);
            }
            catch (Exception ex)
            {
#if DEBUG
                _logger.Error(ex.ToString());
#else
                _logger.Error(ex.Message);
#endif
                _logger.Error("Process stopped due to error");
            }
            finally
            {
                if (ViewModel.InputSource == InputSource.GoogleMyMapsUrl
                    && !string.IsNullOrEmpty(inputFileName)
                    && _file.Exists(inputFileName))
                {
                    _file.Delete(inputFileName);
                }
            }
        }
    }
}
