using System.Net;
using System.Threading.Tasks;

namespace TripToPrint.Core
{
    public interface IWebClientService
    {
        Task<byte[]> DownloadDataAsync(string url);
    }

    public class WebClientService : IWebClientService
    {
        public async Task<byte[]> DownloadDataAsync(string url)
        {
            return await new WebClient().DownloadDataTaskAsync(url);
        }
    }
}
