using System.Linq;
using System.Net;
using System.Threading.Tasks;

using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IHereAdapter
    {
        Task<byte[]> FetchImage(IHaveCoordinate placemark);
        Task<byte[]> FetchOverviewMap(MooiGroup group);
    }

    public class HereAdapter : IHereAdapter
    {
        private const string ROOT_URL = "https://image.maps.cit.api.here.com/mia/1.6";
        private const string MAPVIEW_URL = ROOT_URL + "/mapview?";

        public async Task<byte[]> FetchImage(IHaveCoordinate placemark)
        {
            var url = $"{MAPVIEW_URL}c={placemark.Coordinate.Latitude}%2C{placemark.Coordinate.Longitude}&z=17";

            return await DownloadData(url);
        }

        public async Task<byte[]> FetchOverviewMap(MooiGroup group)
        {
            var poi = string.Join(",", group.Placemarks
                .Select(x => $"{x.Coordinate.Latitude},{x.Coordinate.Longitude}"));
            var url = $"{MAPVIEW_URL}w=800&h=420&poi={poi}";

            return await DownloadData(url);
        }

        private async Task<byte[]> DownloadData(string url)
        {
            return await new WebClient().DownloadDataTaskAsync(url + GetAppCodeUrlPart());
        }

        private string GetAppCodeUrlPart()
        {
            return $"&app_id={Properties.Settings.Default.HereApiAppId}&app_code={Properties.Settings.Default.HereApiAppCode}";
        }
    }
}
