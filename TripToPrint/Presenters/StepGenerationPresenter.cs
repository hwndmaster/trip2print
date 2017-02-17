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
        private readonly IReportGenerator _reportGenerator;
        private readonly ILogStorage _logStorage;
        private readonly ILogger _logger;
        private readonly IWebClientService _webClient;
        private readonly IFileService _file;

        public StepGenerationPresenter(IReportGenerator reportGenerator, ILogStorage logStorage, ILogger logger, IWebClientService webClient, IFileService file)
        {
            _reportGenerator = reportGenerator;
            _logStorage = logStorage;
            _logger = logger;
            _webClient = webClient;
            _file = file;
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
            if (ViewModel.ProgressInPercentage < 100)
            {
                // TODO: Break the process
            }

            return true;
        }

        public Task<bool> BeforeGoNext()
        {
            if (ViewModel.ProgressInPercentage < 100)
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
                var progressTracker = CreateProgressTracker();

                switch (ViewModel.InputSource)
                {
                    case InputSource.LocalFile:
                        inputFileName = ViewModel.InputUri;
                        break;
                    case InputSource.GoogleMyMapsUrl:
                        var inputData = await _webClient.GetAsync(new Uri(ViewModel.InputUri));
                        inputFileName = $"{Path.GetTempPath()}Trip2Print_{Guid.NewGuid()}.kmz";
                        await _file.WriteBytesAsync(inputFileName, inputData);
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

        private IProgressTracker CreateProgressTracker()
        {
            return new ProgressTracker(valueInPercentage => ViewModel.ProgressInPercentage = valueInPercentage);
        }
    }
}
