using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IHereAdapter
    {
        Task<byte[]> FetchThumbnail(MooiPlacemark placemark);
        Task<byte[]> FetchOverviewMap(MooiGroup group);
        Task<List<DiscoveredPlace>> LookupPlaces(KmlPlacemark placemark, string culture, CancellationToken cancellationToken);
    }

    public class HereAdapter : IHereAdapter
    {
        private const string IMAGES_ROOT_URL = "https://image.maps.api.here.com/mia/1.6";
        internal const string IMAGES_MAPVIEW_URL = IMAGES_ROOT_URL + "/mapview?";
        internal const string IMAGES_ROUTE_URL = IMAGES_ROOT_URL + "/route?";
        internal const string IMAGES_ROUTE_ROUTE_PARAM_NAME = "r0";
        internal const string IMAGES_ROUTE_POINT_PARAM_NAME = "m0";

        private const string PLACES_ROOT_URL = "https://places.demo.api.here.com/places/v1";
        internal const string PLACES_DISCOVER_URL = PLACES_ROOT_URL + "/discover/search?";

        internal const string APP_ID_PARAM_NAME = "app_id";
        internal const string APP_CODE_PARAM_NAME = "app_code";

        internal const int TOO_MUCH_OF_COORDINATE_POINTS = 400;
        private const int LOOKUP_PLACES_WITHIN_DISTANCE_IN_METERS = 500;
        private const int MAX_COORDINATE_VALUE_PRECISION = 8;
        private const int COORDINATE_PRECISION_ON_POINTS = 6;
        private const int COORDINATE_PRECISION_ON_ROUTES = 4;
        private const int OVERVIEW_MAP_WIDTH = 1200;
        private const int OVERVIEW_MAP_HEIGHT = 630;
        private const int THUMBNAIL_MAP_ZOOM = 18;
        private const string ACCEPTABLE_LANGUAGE = "en";

        private readonly ILogger _logger;
        private readonly IKmlCalculator _kmlCalculator;
        private readonly IWebClientService _webClient;
        private readonly CultureAgnosticFormatter _formatter;

        public HereAdapter(ILogger logger, IKmlCalculator kmlCalculator, IWebClientService webClient)
        {
            _webClient = webClient;
            _logger = logger;
            _kmlCalculator = kmlCalculator;

            _formatter = new CultureAgnosticFormatter();
        }

        public async Task<byte[]> FetchThumbnail(MooiPlacemark placemark)
        {
            if (placemark.Type == PlacemarkType.Route)
            {
                throw new NotSupportedException("Routes are not supported");
            }

            var url = $"c={CreateStringForCoordinates(null, placemark)}&z={THUMBNAIL_MAP_ZOOM}&ppi=320&w=600&h=390";

            return await DownloadData(IMAGES_MAPVIEW_URL, url);
        }

        public async Task<byte[]> FetchOverviewMap(MooiGroup group)
        {
            var baseUrl = group.Type == GroupType.Routes ? IMAGES_ROUTE_URL : IMAGES_MAPVIEW_URL;
            baseUrl = $"{baseUrl}w={OVERVIEW_MAP_WIDTH}&h={OVERVIEW_MAP_HEIGHT}&sb=k&ppi=250";

            var parameters = string.Empty;
            if (group.Type == GroupType.Routes)
            {
                var route = GetAndTrimRouteCoordinates(group);
                var routeCoords = CreateStringForCoordinates(COORDINATE_PRECISION_ON_ROUTES, route);

                parameters += $"&{IMAGES_ROUTE_ROUTE_PARAM_NAME}={routeCoords}";
                parameters += $"&{IMAGES_ROUTE_POINT_PARAM_NAME}=" + CreateStringForCoordinates(COORDINATE_PRECISION_ON_POINTS, group.Placemarks
                                  .Where(x => x.Type == PlacemarkType.Point)
                                  .Cast<IHaveCoordinates>()
                                  .ToArray());
                parameters += "&lc0=navy&sc0=888888";
            }
            else
            {
                var poi = CreateStringForCoordinates(null, group.Placemarks.Cast<IHaveCoordinates>().ToArray());
                parameters += $"&poi={poi}&poitxc=black&poifc=yellow&poitxs=15";

                if (group.Placemarks.Count == 1)
                {
                    parameters += "&z=14";
                }
            }

            return await DownloadData(baseUrl, parameters);
        }

        public async Task<List<DiscoveredPlace>> LookupPlaces(KmlPlacemark placemark, string culture, CancellationToken cancellationToken)
        {
            var discoveredPlaces = new List<DiscoveredPlace>();
            try
            {
                if (_kmlCalculator.PlacemarkIsShape(placemark))
                {
                    return discoveredPlaces;
                }
                var coord = CreateStringForCoordinates(null, placemark);
                var url = $"{PLACES_DISCOVER_URL}at={coord}&refinements=true&size=4&q=" + Uri.EscapeUriString(placemark.Name);

                // Refer to: https://developer.here.com/rest-apis/documentation/places/topics_api/resource-search.html
                var jsonValue = await DownloadString(url, culture, cancellationToken);
                if (jsonValue == null)
                    return discoveredPlaces;

                dynamic json = JObject.Parse(jsonValue);
                foreach (var place in json.results.items)
                {
                    if (Convert.ToInt32(place.distance) > LOOKUP_PLACES_WITHIN_DISTANCE_IN_METERS)
                        continue;

                    var discoveredPlace = new DiscoveredPlace {
                        Title = (string) place.title,
                        Coordinate = new GeoCoordinate(Convert.ToDouble(place.position[0]), Convert.ToDouble(place.position[1])),
                        Address = ReplaceHtmlNewLines((string) place.vicinity),
                        AverageRating = Convert.ToDouble(place.averageRating),
                        IconUrl = new Uri((string) place.icon),
                        OpeningHours = ReplaceHtmlNewLines((string)place.openingHours?.text)
                    };

                    if (place.href != null)
                    {
                        var extraInfoUrl = (string) place.href;
                        extraInfoUrl += "&show_content=wikipedia";
                        var jsonValueExtra = await DownloadString(extraInfoUrl, culture, cancellationToken);
                        if (jsonValueExtra != null)
                        {
                            dynamic jsonExtra = JObject.Parse(jsonValueExtra);
                            if (jsonExtra.contacts != null)
                            {
                                var phonesArray = jsonExtra.contacts.phone as JArray;
                                if (phonesArray != null)
                                {
                                    dynamic phone = phonesArray[0];
                                    discoveredPlace.ContactPhone = (string) phone.value;
                                }

                                var websitesArray = jsonExtra.contacts.website as JArray;
                                if (websitesArray != null)
                                {
                                    dynamic website = websitesArray[0];
                                    discoveredPlace.Website = (string) website.value;
                                }
                            }
                            var editorialsArray = jsonExtra.media?.editorials?.items as JArray;
                            if (editorialsArray != null)
                            {
                                // TODO: Что если элементов больше, чем 1?
                                var editorialLanguage = (string) ((dynamic) editorialsArray[0]).language;
                                var requestedLanguage = culture.Split('-')[0];
                                if (editorialLanguage.Equals(requestedLanguage, StringComparison.OrdinalIgnoreCase)
                                    || editorialLanguage.Equals(ACCEPTABLE_LANGUAGE, StringComparison.OrdinalIgnoreCase))
                                {
                                    discoveredPlace.WikipediaContent = FilterContent((string)((dynamic)editorialsArray[0]).description);
                                }
                            }
                        }
                    }

                    if (discoveredPlace.IsUseless())
                    {
                        continue;
                    }

                    discoveredPlaces.Add(discoveredPlace);
                }
            }
            catch (Exception e)
            {
                _logger.Exception(e);
            }

            return discoveredPlaces;
        }

        internal async Task<string> DownloadString(string url, string language, CancellationToken cancellationToken)
        {
            return await _webClient.GetStringAsync(new Uri(url + GetAppCodeUrlPart()),
                cancellationToken,
                new Dictionary<HttpRequestHeader, string> {
                    { HttpRequestHeader.AcceptLanguage, language }
                });
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

        private string ReplaceHtmlNewLines(string html)
        {
            if (html == null)
                return null;

            return Regex.Replace(html, @"<br\s*/?>", ", ");
        }

        private string FilterContent(string content)
        {
            return Regex.Replace(content, @"</?p>", " ");
        }
    }
}
