using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TripToPrint.Core
{
    public interface IWebClientService
    {
        Task<byte[]> GetAsync(Uri url);
        Task<byte[]> PostAsync(Uri url, string parameters);
    }

    public class WebClientService : IWebClientService
    {
        public async Task<byte[]> GetAsync(Uri url)
        {
            using (var webClient = new WebClient())
            {
                return await webClient.DownloadDataTaskAsync(url);
            }
        }

        public async Task<byte[]> PostAsync(Uri url, string parameters)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var data = Encoding.Default.GetBytes(parameters);

                return await webClient.UploadDataTaskAsync(url, data);
            }
        }
    }
}
