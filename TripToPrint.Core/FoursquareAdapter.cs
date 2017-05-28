using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;
using TripToPrint.Core.Models.Venues;

namespace TripToPrint.Core
{
    public interface IFoursquareAdapter
    {
        Task<FoursquareVenue> LookupMatchingVenue(KmlPlacemark placemark, string language, CancellationToken cancellationToken);
        Task<List<FoursquareVenue>> ExplorePopular(KmlPlacemark placemark, string language, CancellationToken cancellationToken);
    }

    public class FoursquareAdapter : IFoursquareAdapter
    {
        private readonly ILogger _logger;
        private readonly IWebClientService _webClient;
        private readonly IKmlCalculator _kmlCalculator;
        private readonly CultureAgnosticFormatter _formatter = new CultureAgnosticFormatter();

        private const string ROOT_URL = "https://api.foursquare.com/v2/";
        private const string VENUES_URL = ROOT_URL + "venues/";
        private const string VENUES_SEARCH_URL = VENUES_URL + "search?";
        private const string VENUES_EXPLORE_URL = VENUES_URL + "explore?";
        private const string VERSION_DATE = "20170526";
        private const int LIMIT_LOOKUP = 1;
        private const int LIMIT_EXPLORE = 8;
        private const int VENUE_MAX_RATING = 10;
        private const int VENUE_MAX_PRICE_LEVEL = 4;
        private const int VENUE_ICON_SIZE = 32;
        private const string VENUE_PHOTO_SIZE = "300x300";

        internal const string CLIENT_ID_PARAM_NAME = "client_id";
        internal const string CLIENT_SECRET_PARAM_NAME = "client_secret";

        public FoursquareAdapter(ILogger logger, IWebClientService webClient, IKmlCalculator kmlCalculator)
        {
            _logger = logger;
            _webClient = webClient;
            _kmlCalculator = kmlCalculator;
        }

        public async Task<FoursquareVenue> LookupMatchingVenue(KmlPlacemark placemark, string language, CancellationToken cancellationToken)
        {
            if (placemark.Coordinates.Length == 0
                || _kmlCalculator.PlacemarkIsShape(placemark))
            {
                return null;
            }

            var coord = _formatter.FormatCoordinates(null, placemark);
            var url = $"{VENUES_SEARCH_URL}ll={coord}&query={Uri.EscapeUriString(placemark.Name)}&intent=match&limit={LIMIT_LOOKUP}";
            var data = await DownloadString(url, language, cancellationToken);
            if (data == null)
            {
                return null;
            }
            dynamic json = JObject.Parse(data);
            if (!CheckMeta(url, json))
            {
                return null;
            }
            if (((JArray) json.response.venues).Count == 0)
            {
                return null;
            }

            dynamic jsonVenue = json.response.venues[0];

            var venueDetailsUrl = $"{VENUES_URL}" + jsonVenue.id;
            data = await DownloadString(venueDetailsUrl, language, cancellationToken);
            json = JObject.Parse(data);
            if (CheckMeta(url, json))
            {
                jsonVenue = json.response.venue;
            }

            return CreateVenueModel(jsonVenue);
        }

        public async Task<List<FoursquareVenue>> ExplorePopular(KmlPlacemark placemark, string language, CancellationToken cancellationToken)
        {
            // TODO: Add support of shapes

            if (placemark.Coordinates.Length == 0
                || _kmlCalculator.PlacemarkIsShape(placemark))
            {
                return null;
            }

            var coord = _formatter.FormatCoordinates(null, placemark);
            var url = $"{VENUES_EXPLORE_URL}ll={coord}&limit={LIMIT_EXPLORE}";
            var data = await DownloadString(url, language, cancellationToken);
            if (data == null)
            {
                return null;
            }
            dynamic json = JObject.Parse(data);
            if (!CheckMeta(url, json))
            {
                return null;
            }
            if (((JArray)json.response.groups).Count == 0)
            {
                return null;
            }
            var jsonItems = (JArray)json.response.groups[0].items;

            return jsonItems.Select<JToken, FoursquareVenue>(x => CreateVenueModel(((dynamic)x).venue, placemark.Coordinates[0])).ToList();
        }

        private async Task<string> DownloadString(string url, string language, CancellationToken cancellationToken)
        {
            return await _webClient.GetStringAsync(new Uri(url + GetAppCodeUrlPart(url)),
                cancellationToken,
                new Dictionary<HttpRequestHeader, string> {
                    { HttpRequestHeader.AcceptLanguage, language }
                });
        }

        private bool CheckMeta(string url, dynamic json)
        {
            if (json.meta.code == 200)
            {
                return true;
            }

            _logger.Error($"Foursquare request failed on url={{{url}}}: {json.meta.errorType}. {json.meta.errorDetail}");

            return false;
        }

        private FoursquareVenue CreateVenueModel(dynamic jsonVenue, GeoCoordinate originCoordinate = null)
        {
            var facebookUser = (string)(jsonVenue.contact?.facebookUsername ?? jsonVenue.contact?.facebook);
            var facebookUrl = string.IsNullOrEmpty(facebookUser) ? null : $"https://www.facebook.com/{facebookUser}";
            var mainCategory = jsonVenue.categories[0];
            var coordinate = new GeoCoordinate(Convert.ToDouble(jsonVenue.location.lat), Convert.ToDouble(jsonVenue.location.lng));
            const int takeTopCategories = 2;

            var venue = new FoursquareVenue {
                Title = jsonVenue.name,
                Category = string.Join(", ", ((JArray) jsonVenue.categories).Select(x => ((dynamic) x).shortName).Take(takeTopCategories)),
                Coordinate = coordinate,
                Rating = jsonVenue.rating,
                RatingColor = jsonVenue.ratingColor,
                MaxRating = VENUE_MAX_RATING,
                LikesCount = jsonVenue.likes?.count,
                DistanceToPlacemark = _kmlCalculator.GetDistanceInMeters(originCoordinate, coordinate),
                Region = string.Join(", ", new[] {
                    (string) jsonVenue.location?.city,
                    (string) jsonVenue.location?.state,
                    (string) jsonVenue.location?.country
                }.Where(x => x != null)),
                Address = jsonVenue.location?.address == null ? null : string.Join(", ", ((JArray) jsonVenue.location?.formattedAddress ?? new JArray()).Values<string>().ToArray()),
                ContactPhone = jsonVenue.contact?.formattedPhone,
                Websites = new string[] {
                    jsonVenue.url,
                    jsonVenue.shortUrl,
                    facebookUrl
                }.Where(x => !string.IsNullOrEmpty(x)).ToArray(),
                IconUrl = new Uri($"{mainCategory.icon.prefix}{VENUE_ICON_SIZE}{mainCategory.icon.suffix}"),
                PriceLevel = jsonVenue.price?.tier,
                PriceCurrency = jsonVenue.price?.currency,
                PriceMaxLevel = VENUE_MAX_PRICE_LEVEL
            };

            if ("public".Equals((string)jsonVenue.bestPhoto?.visibility, StringComparison.OrdinalIgnoreCase))
            {
                venue.PhotoUrls = new[] {
                    $"{jsonVenue.bestPhoto.prefix}{VENUE_PHOTO_SIZE}{jsonVenue.bestPhoto.suffix}"
                };
            }

            if (jsonVenue.hours?.timeframes != null)
            {
                var sb = new StringBuilder();
                foreach (dynamic timeframe in (JArray) jsonVenue.hours.timeframes)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append((string) timeframe.days);
                    sb.Append(": ");
                    var openingHours = ((JArray) timeframe.open).Select(o => ((dynamic) o).renderedTime);
                    sb.Append(string.Join(", ", openingHours));
                }
                venue.OpeningHours = sb.ToString();
            }

            if (jsonVenue.phrases != null)
            {
                venue.Phrases = ((JArray) jsonVenue.phrases).Select(x => (string) ((dynamic) x).phrase).ToArray();
            }

            if (jsonVenue.tags != null)
            {
                venue.Tags = ((JArray)jsonVenue.tags).Values<string>().ToArray();
            }

            if (jsonVenue.tips != null)
            {
                var jsonTips = (JArray) ((dynamic) ((JArray) jsonVenue.tips.groups)[0]).items;

                venue.Tips = jsonTips.Take(5).Select(x => {
                    dynamic tip = x;
                    return new FoursquareVenueTip {
                        Message = (string)tip.text,
                        Likes = (int)tip.likes.count,
                        AgreeCount = (int)tip.agreeCount,
                        DisagreeCount = (int)tip.disagreeCount
                    };
                }).ToArray();
            }


            return venue;
        }

        private static string GetAppCodeUrlPart(string url)
        {
            var clientId = Properties.Settings.Default.FoursquareApiClientId;
            var clientSecret = Properties.Settings.Default.FoursquareApiClientSecret;
            var sep = url.Contains("?") ? "&" : "?";
            return $"{sep}v={VERSION_DATE}&{CLIENT_ID_PARAM_NAME}={clientId}&{CLIENT_SECRET_PARAM_NAME}={clientSecret}";
        }
    }
}
