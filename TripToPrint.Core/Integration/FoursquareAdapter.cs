using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using TripToPrint.Core.ExtensionMethods;
using TripToPrint.Core.Integration.Models;
using TripToPrint.Core.Logging;
using TripToPrint.Core.Models;
using TripToPrint.Core.Models.Venues;

namespace TripToPrint.Core.Integration
{
    public interface IFoursquareAdapter
    {
        Task<FoursquareVenue> LookupMatchingVenue(KmlPlacemark placemark, string language, CancellationToken cancellationToken);
        Task<List<FoursquareVenue>> ExplorePopular(KmlPlacemark placemark, string language, CancellationToken cancellationToken);
        Task PopulateWithDetailedInfo(IEnumerable<DiscoveredPlace> discoveredPlaces, string language, CancellationToken cancellationToken);
    }

    internal class FoursquareAdapter : IFoursquareAdapter
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
        private const int TOP_TIPS_TO_GET = 3;
        private const int VENUE_MAX_RATING = 10;
        private const int VENUE_MAX_PRICE_LEVEL = 4;
        private const int VENUE_ICON_SIZE = 32;
        private const string VENUE_PHOTO_SIZE = "300x300";
        private const int MAX_SIMULTANEOUS_HTTP_REQUESTS = 4;
        private const string VENUE_WEB_URL = "https://foursquare.com/v/";

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

            FoursquareResponseVenue venue;
            var venueUrl = ExtractFoursquareUrlFromDescription(placemark);

            if (string.IsNullOrEmpty(venueUrl))
            {
                var coord = _formatter.FormatCoordinates(null, placemark);
                var url = $"{VENUES_SEARCH_URL}ll={coord}&query={Uri.EscapeUriString(placemark.Name)}&intent=match&limit={LIMIT_LOOKUP}";
                var data = await DownloadString(url, language, cancellationToken);
                if (data == null)
                {
                    return null;
                }
                var response = JsonConvert.DeserializeObject<FoursquareResponse<FoursquareSearchResponseBody>>(data);
                if (!CheckMeta(url, response))
                {
                    return null;
                }
                if (response.Response.Venues.Length == 0)
                {
                    return null;
                }
                venue = response.Response.Venues[0];
                venue = await DownloadVenueDetails(venue.Id, language, cancellationToken) ?? venue;
            }
            else
            {
                var venueId = ExtractVenueIdFromUrl(venueUrl);
                venue = await DownloadVenueDetails(venueId, language, cancellationToken);

                placemark.Description = placemark.Description.Replace(venueUrl, string.Empty).Trim();
            }

            return CreateVenueModel(venue);
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

            var response = JsonConvert.DeserializeObject<FoursquareResponse<FoursquareExploreResponseBody>>(data);
            if (!CheckMeta(url, response))
            {
                return null;
            }
            if (response.Response?.Groups.Length == 0)
            {
                return null;
            }

            return response.Response?.Groups[0].Items
                .Take(LIMIT_EXPLORE)
                .Select(x => CreateVenueModel(x.Venue, placemark.Coordinates[0]))
                .ToList();
        }

        public async Task PopulateWithDetailedInfo(IEnumerable<DiscoveredPlace> discoveredPlaces, string language, CancellationToken cancellationToken)
        {
            await discoveredPlaces.ForEachAsync(MAX_SIMULTANEOUS_HTTP_REQUESTS, async (place) => {
                var venue = (FoursquareVenue) place.Venue;

                var venueDetails = await DownloadVenueDetails(venue.Id, language, cancellationToken);
                if (venueDetails != null)
                {
                    place.Venue = CreateVenueModel(venueDetails);
                }
            });
        }

        private async Task<string> DownloadString(string url, string language, CancellationToken cancellationToken)
        {
            return await _webClient.GetStringAsync(new Uri(url + GetAppCodeUrlPart(url)),
                cancellationToken,
                new Dictionary<HttpRequestHeader, string> {
                    { HttpRequestHeader.AcceptLanguage, language }
                });
        }

        private bool CheckMeta<T>(string url, FoursquareResponse<T> response)
        {
            if (response.Meta.Code == 200)
            {
                return true;
            }

            _logger.Error($"Foursquare request failed on url={{{url}}}: {response.Meta.ErrorType}. {response.Meta.ErrorDetail}");

            return false;
        }

        private async Task<FoursquareResponseVenue> DownloadVenueDetails(string id, string language, CancellationToken cancellationToken)
        {
            var venueDetailsUrl = $"{VENUES_URL}{id}";
            var response = await DownloadString(venueDetailsUrl, language, cancellationToken);
            var detailedVenueResponse = JsonConvert.DeserializeObject<FoursquareResponse<FoursquareDetailsResponseBody>>(response);
            if (CheckMeta(venueDetailsUrl, detailedVenueResponse))
            {
                return detailedVenueResponse.Response.Venue;
            }
            return null;
        }

        private FoursquareVenue CreateVenueModel(FoursquareResponseVenue responseVenue, GeoCoordinate originCoordinate = null)
        {
            var facebookUser = responseVenue.Contact?.FacebookUsername ?? responseVenue.Contact?.Facebook;
            var facebookUrl = string.IsNullOrEmpty(facebookUser) ? null : $"https://www.facebook.com/{facebookUser}";
            var mainCategory = responseVenue.Categories.FirstOrDefault(x => x.Primary) ?? responseVenue.Categories[0];
            var coordinate = new GeoCoordinate(responseVenue.Location.Lat, responseVenue.Location.Lng);
            const int takeTopCategories = 2;

            var venue = new FoursquareVenue {
                Id = responseVenue.Id,
                Title = responseVenue.Name,
                Category = string.Join(", ", responseVenue.Categories
                    .OrderBy(x => x.Primary ? 1 : 2)
                    .Select(x => x.ShortName)
                    .Take(takeTopCategories)),
                Coordinate = coordinate,
                Rating = responseVenue.Rating,
                RatingColor = responseVenue.RatingColor,
                MaxRating = VENUE_MAX_RATING,
                LikesCount = responseVenue.Likes?.Count,
                DistanceToPlacemark = _kmlCalculator.GetDistanceInMeters(originCoordinate, coordinate),
                Region = string.Join(", ", new[] {
                    responseVenue.Location?.City,
                    responseVenue.Location?.State,
                    responseVenue.Location?.Country
                }.Where(x => x != null)),
                Address = responseVenue.Location?.Address == null
                    ? null
                    : string.Join(", ", responseVenue.Location?.FormattedAddress ?? new string[0]),
                ContactPhone = responseVenue.Contact?.FormattedPhone,
                Websites = new[] {
                    responseVenue.ShortUrl,
                    responseVenue.Url,
                    facebookUrl
                }.Where(x => !string.IsNullOrEmpty(x)).ToArray(),
                IconUrl = new Uri($"{mainCategory.Icon.Prefix}{VENUE_ICON_SIZE}{mainCategory.Icon.Suffix}"),
                PriceLevel = responseVenue.Price?.Tier,
                PriceCurrency = responseVenue.Price?.Currency,
                PriceMaxLevel = VENUE_MAX_PRICE_LEVEL,
                Tags = responseVenue.Tags
            };

            if ("public".Equals(responseVenue.BestPhoto?.Visibility, StringComparison.OrdinalIgnoreCase))
            {
                venue.PhotoUrls = new[] {
                    $"{responseVenue.BestPhoto?.Prefix}{VENUE_PHOTO_SIZE}{responseVenue.BestPhoto?.Suffix}"
                };
            }

            if (responseVenue.Hours?.Timeframes != null)
            {
                var sb = new StringBuilder();
                foreach (var timeframe in responseVenue.Hours.Timeframes)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append($"{timeframe.Days}: ");
                    sb.Append(string.Join(", ", timeframe.Open.Select(o => o.RenderedTime)));
                }
                venue.OpeningHours = sb.ToString();
            }

            if (responseVenue.Tips != null)
            {
                var tips = responseVenue.Tips.Groups[0].Items;

                venue.Tips = tips
                    .Where(x => x.AgreeCount > 0)
                    .Take(TOP_TIPS_TO_GET)
                    .Select(tip => new FoursquareVenueTip {
                        Message = tip.Text,
                        Likes = tip.Likes.Count,
                        AgreeCount = tip.AgreeCount,
                        DisagreeCount = tip.DisagreeCount
                    }).ToArray();
            }

            if (venue.Websites.Length == 0)
            {
                venue.Websites = new[] {
                    $"{VENUE_WEB_URL}{venue.Id}"
                };
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

        private string ExtractVenueIdFromUrl(string venueUrl)
        {
            var match = Regex.Match(venueUrl, @"/v/.+?/(?<id>\w+)");
            if (match.Success)
            {
                return match.Groups["id"].Value;
            }
            throw new InvalidOperationException("A venue ID from the following Foursquare URL cannot be recognized: " + venueUrl);
        }

        private string ExtractFoursquareUrlFromDescription(KmlPlacemark placemark)
        {
            // Example: https://foursquare.com/v/barcomis-deli/4af818bef964a520f70a22e3
            if (string.IsNullOrEmpty(placemark.Description))
            {
                return null;
            }
            var match = Regex.Match(placemark.Description, @"https?://foursquare\.com/v/.+?/.+?/?(?=\s|$)");
            if (match.Success)
            {
                return match.Value;
            }
            return null;
        }
    }
}
