using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;
using TripToPrint.Core.Models.Integration;
using TripToPrint.Core.Models.Venues;

namespace TripToPrint.Core
{
    public interface IHereAdapter
    {
        Task<byte[]> FetchThumbnail(MooiPlacemark placemark);
        Task<byte[]> FetchOverviewMap(MooiGroup group);
        Task<VenueBase> LookupMatchingVenue(KmlPlacemark placemark, string culture, CancellationToken cancellationToken);
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
        private const int LOOKUP_PLACES_WITHIN_DISTANCE_IN_METERS = 300;
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

            var url = $"c={_formatter.FormatCoordinates(null, placemark)}&z={THUMBNAIL_MAP_ZOOM}&ppi=320&w=600&h=390";

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
                var routeCoords = _formatter.FormatCoordinates(COORDINATE_PRECISION_ON_ROUTES, route);

                parameters += $"&{IMAGES_ROUTE_ROUTE_PARAM_NAME}={routeCoords}";
                parameters += $"&{IMAGES_ROUTE_POINT_PARAM_NAME}=" + _formatter.FormatCoordinates(COORDINATE_PRECISION_ON_POINTS, group.Placemarks
                                  .Where(x => x.Type == PlacemarkType.Point)
                                  .Cast<IHasCoordinates>()
                                  .ToArray());
                parameters += "&lc0=navy&sc0=888888";
            }
            else
            {
                var poi = _formatter.FormatCoordinates(null, group.Placemarks.Cast<IHasCoordinates>().ToArray());
                parameters += $"&poi={poi}&poitxc=black&poifc=yellow&poitxs=15";

                if (group.Placemarks.Count == 1)
                {
                    parameters += "&z=14";
                }
            }

            return await DownloadData(baseUrl, parameters);
        }

        public async Task<VenueBase> LookupMatchingVenue(KmlPlacemark placemark, string culture, CancellationToken cancellationToken)
        {
            try
            {
                if (placemark.Coordinates.Length == 0)
                {
                    return null;
                }
                if (_kmlCalculator.PlacemarkIsShape(placemark))
                {
                    return null;
                }
                var coord = _formatter.FormatCoordinates(null, placemark);
                var url = $"{PLACES_DISCOVER_URL}at={coord}&refinements=true&size=4&q={Uri.EscapeUriString(placemark.Name)}";

                // Refer to: https://developer.here.com/rest-apis/documentation/places/topics_api/resource-search.html
                var jsonValue = await DownloadString(url, culture, cancellationToken);
                if (jsonValue == null)
                {
                    return null;
                }

                var response = JsonConvert.DeserializeObject<HereDiscoverSearchResponse>(jsonValue);
                var place = response.Results.Items.FirstOrDefault(x => x.Distance < LOOKUP_PLACES_WITHIN_DISTANCE_IN_METERS);
                if (place == null)
                {
                    return null;
                }

                var discoveredPlace = new HereVenue {
                    Id = place.Id,
                    Title = place.Title,
                    Coordinate = new GeoCoordinate(place.Position[0], place.Position[1]),
                    Address = ReplaceHtmlNewLines(place.Vicinity),
                    IconUrl = new Uri(place.Icon),
                    OpeningHours = ReplaceHtmlNewLines(place.OpeningHours?.Text),
                    Category = place.Category.Title,
                    Websites = new string[0]
                };

                if (place.Href != null)
                {
                    var extraInfoUrl = place.Href + "&show_content=wikipedia";
                    var jsonValueExtra = await DownloadString(extraInfoUrl, culture, cancellationToken);
                    if (jsonValueExtra != null)
                    {
                        var extra = JsonConvert.DeserializeObject<HerePlace>(jsonValueExtra);

                        if (extra.Location?.Address != null)
                        {
                            discoveredPlace.Region = string.Join(", ", extra.Location.Address.State, extra.Location.Address.Country);
                        }

                        discoveredPlace.Tags = extra.Tags?.Select(x => x.Title).ToArray();

                        if (!string.IsNullOrEmpty(extra.View))
                        {
                            discoveredPlace.Websites = new [] { extra.View };
                        }

                        if (extra.Contacts?.Phone?.Length > 0)
                        {
                            discoveredPlace.ContactPhone = extra.Contacts.Phone[0].Value;
                        }

                        if (extra.Contacts?.Website?.Length > 0)
                        {
                            discoveredPlace.Websites = discoveredPlace.Websites
                                .Concat(extra.Contacts.Website.Select(x => x.Value)).ToArray();
                        }

                        var wikipedia = extra.Media?.Editorials?.Items
                            .FirstOrDefault(x => x.Supplier.Id.Equals("wikipedia", StringComparison.OrdinalIgnoreCase));
                        if (wikipedia != null)
                        {
                            var requestedLanguage = culture.Split('-')[0];
                            if (wikipedia.Language.Equals(requestedLanguage, StringComparison.OrdinalIgnoreCase)
                                || wikipedia.Language.Equals(ACCEPTABLE_LANGUAGE, StringComparison.OrdinalIgnoreCase))
                            {
                                discoveredPlace.WikipediaContent = FilterContent(wikipedia.Description);
                            }
                        }
                    }
                }

                if (discoveredPlace.IsUseless())
                {
                    return null;
                }

                return discoveredPlace;
            }
            catch (Exception e)
            {
                _logger.Exception(e);
                return null;
            }
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

        internal IHasCoordinates GetAndTrimRouteCoordinates(MooiGroup group)
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
