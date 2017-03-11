using System;
using System.Linq;
using System.Threading.Tasks;
using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IHereAdapter
    {
        Task<byte[]> FetchThumbnail(MooiPlacemark placemark);
        Task<byte[]> FetchOverviewMap(MooiGroup group);
    }

    public class HereAdapter : IHereAdapter
    {
        private const string ROOT_URL = "https://image.maps.api.here.com/mia/1.6";
        internal const string MAPVIEW_URL = ROOT_URL + "/mapview?";
        internal const string ROUTE_URL = ROOT_URL + "/route?";
        internal const string ROUTE_ROUTE_PARAM_NAME = "r0";
        internal const string ROUTE_POINT_PARAM_NAME = "m0";
        internal const string APP_ID_PARAM_NAME = "app_id";
        internal const string APP_CODE_PARAM_NAME = "app_code";
        internal const int TOO_MUCH_OF_COORDINATE_POINTS = 400;
        private const int MAX_COORDINATE_VALUE_PRECISION = 8;
        private const int COORDINATE_PRECISION_ON_POINTS = 6;
        private const int COORDINATE_PRECISION_ON_ROUTES = 4;
        private const int OVERVIEW_MAP_WIDTH = 800;
        private const int OVERVIEW_MAP_HEIGHT = 420;
        private const int THUMBNAIL_MAP_ZOOM = 17;

        private readonly IWebClientService _webClient;
        private readonly CultureAgnosticFormatter _formatter;

        public HereAdapter(IWebClientService webClient)
        {
            _webClient = webClient;
            _formatter = new CultureAgnosticFormatter();
        }

        public async Task<byte[]> FetchThumbnail(MooiPlacemark placemark)
        {
            if (placemark.Type == PlacemarkType.Route)
            {
                throw new NotSupportedException("Routes are not supported");
            }

            var url = $"c={CreateStringForCoordinates(null, placemark)}&z={THUMBNAIL_MAP_ZOOM}";

            return await DownloadData(MAPVIEW_URL, url);
        }

        public async Task<byte[]> FetchOverviewMap(MooiGroup group)
        {
            var baseUrl = group.Type == GroupType.Routes ? ROUTE_URL : MAPVIEW_URL;
            baseUrl = $"{baseUrl}w={OVERVIEW_MAP_WIDTH}&h={OVERVIEW_MAP_HEIGHT}&sb=k";

            var parameters = string.Empty;
            if (group.Type == GroupType.Routes)
            {
                var route = GetAndTrimRouteCoordinates(group);
                var routeCoords = CreateStringForCoordinates(COORDINATE_PRECISION_ON_ROUTES, route);

                parameters += $"&{ROUTE_ROUTE_PARAM_NAME}={routeCoords}";
                parameters += $"&{ROUTE_POINT_PARAM_NAME}=" + CreateStringForCoordinates(COORDINATE_PRECISION_ON_POINTS, group.Placemarks
                                  .Where(x => x.Type == PlacemarkType.Point)
                                  .Cast<IHaveCoordinates>()
                                  .ToArray());
                parameters += "&lc0=yellow&sc0=888888";
            }
            else
            {
                var poi = CreateStringForCoordinates(null, group.Placemarks.Cast<IHaveCoordinates>().ToArray());
                parameters += $"&poi={poi}&poitxc=black&poifc=yellow&poitxs=12";
            }

            return await DownloadData(baseUrl, parameters);
        }

        internal async Task<byte[]> DownloadData(string url, string parameters)
        {
            return await _webClient.PostAsync(new Uri(url + GetAppCodeUrlPart()), parameters);
        }

        internal IHaveCoordinates GetAndTrimRouteCoordinates(MooiGroup group)
        {
            var route = group.Placemarks.Single(x => x.Type == PlacemarkType.Route);

            if (route.Coordinates.Length > TOO_MUCH_OF_COORDINATE_POINTS)
            {
                var factor = (double)route.Coordinates.Length / TOO_MUCH_OF_COORDINATE_POINTS;
                var coords = route.Coordinates
                    .Select((coord, i) => new { rank = Math.Floor(i / factor), coord })
                    .GroupBy(x => x.rank)
                    .Select(x => x.First().coord);

                return new MooiPlacemark { Coordinates = coords.ToArray() };
            }

            return route;
        }

        private string CreateStringForCoordinates(int? precision, params IHaveCoordinates[] placemarks)
        {
            precision = precision ?? MAX_COORDINATE_VALUE_PRECISION;
            return string.Join(",", placemarks
                .SelectMany(x => x.Coordinates)
                .Select(x => $"{_formatter.Format(x.Latitude, precision.Value)},{_formatter.Format(x.Longitude, precision.Value)}")
                .Distinct());
        }

        private string GetAppCodeUrlPart()
        {
            var appId = Properties.Settings.Default.HereApiAppId;
            var appCode = Properties.Settings.Default.HereApiAppCode;
            return $"&{APP_ID_PARAM_NAME}={appId}&{APP_CODE_PARAM_NAME}={appCode}";
        }
    }
}
