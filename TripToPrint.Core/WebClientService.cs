using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TripToPrint.Core.Logging;

namespace TripToPrint.Core
{
    public interface IWebClientService
    {
        Task<string> GetStringAsync(Uri url, CancellationToken cancellationToken, Dictionary<HttpRequestHeader, string> headers = null);
        Task<byte[]> GetAsync(Uri url);
        Task<byte[]> PostAsync(Uri url, string parameters);
    }

    [ExcludeFromCodeCoverage]
    internal class WebClientService : IWebClientService
    {
        private readonly ILogger _logger;

        public WebClientService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<string> GetStringAsync(Uri url, CancellationToken cancellationToken, Dictionary<HttpRequestHeader, string> headers = null)
        {
#if DEBUG
            var cached = TestingEnvCore.GetUrlString(url);
            if (cached != null)
                return cached;
#endif

            try
            {
                using (var webClient = new WebClient())
                using (cancellationToken.Register(() => webClient.CancelAsync()))
                {
                    AppendHeaders(webClient, headers);
                    webClient.Encoding = Encoding.UTF8;
                    var result = await webClient.DownloadStringTaskAsync(url);
#if DEBUG
                    TestingEnvCore.StoreUrlString(url, result);
#endif
                    return result;
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.RequestCanceled)
            {
                // Ignore this exception
                return null;
            }
            catch (Exception e)
            {
                _logger.Exception(e);
                return null;
            }
        }

        public async Task<byte[]> GetAsync(Uri url)
        {
#if DEBUG
            var cached = TestingEnvCore.GetUrlBytes(url);
            if (cached != null)
                return cached;
#endif

            try
            {
                using (var webClient = new WebClient())
                {
                    var result = await webClient.DownloadDataTaskAsync(url);
#if DEBUG
                    TestingEnvCore.StoreUrlBytes(url, null, result);
#endif
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.Exception(e);
                return null;
            }
        }

        public async Task<byte[]> PostAsync(Uri url, string parameters)
        {
#if DEBUG
            var cached = TestingEnvCore.GetUrlBytes(url, parameters);
            if (cached != null)
                return cached;
#endif
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var data = Encoding.Default.GetBytes(parameters);

                    var result = await webClient.UploadDataTaskAsync(url, data);

#if DEBUG
                    TestingEnvCore.StoreUrlBytes(url, parameters, result);
#endif

                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.Exception(e);
                return null;
            }
        }

        private void AppendHeaders(WebClient webClient, Dictionary<HttpRequestHeader, string> headers)
        {
            if (headers == null)
                return;

            foreach (var header in headers)
            {
                webClient.Headers.Add(header.Key, header.Value);
            }
        }
    }
}
