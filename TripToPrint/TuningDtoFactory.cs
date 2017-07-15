using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;

using TripToPrint.Core;
using TripToPrint.Core.Models;
using TripToPrint.Core.Models.Venues;
using TripToPrint.ReportTuning.Dto;

namespace TripToPrint
{
    public interface ITuningDtoFactory
    {
        MooiDocumentDto Create(MooiDocument document, string reportTempPath);
    }

    public sealed class TuningDtoFactory : ITuningDtoFactory
    {
        private readonly CultureAgnosticFormatter _formatter = new CultureAgnosticFormatter();

        private readonly Dictionary<VenueSource, Func<VenueBase, VenueBaseDto>> _venueConverters;

        private const int COORDINATE_VALUE_PRECISION = 6;

        public TuningDtoFactory()
        {
            _venueConverters = new Dictionary<VenueSource, Func<VenueBase, VenueBaseDto>> {
                { VenueSource.Here, CreateHereVenue },
                { VenueSource.Foursquare, CreateFoursquareVenue }
            };
        }

        public MooiDocumentDto Create(MooiDocument document, string reportTempPath)
        {
            return new MooiDocumentDto {
                Title = document.Title,
                Description = document.Description,
                Sections = document.Sections.Select(CreateSection).ToArray()
            };
        }

        private MooiSectionDto CreateSection(MooiSection section)
        {
            return new MooiSectionDto {
                Name = section.Name,
                Clusters = section.Clusters.Select(CreateCluster).ToArray()
            };
        }

        private MooiClusterDto CreateCluster(MooiCluster cluster)
        {
            return new MooiClusterDto {
                Id = cluster.Id,
                IsRoute = cluster.Type == ClusterType.Routes,
                OverviewMapFilePath = ConvertToLocalFileUrl(cluster.OverviewMapFilePath),
                Placemarks = cluster.Placemarks.Select(CreatePlacemark).ToArray()
            };
        }

        private MooiPlacemarkDto CreatePlacemark(MooiPlacemark placemark)
        {
            var pm = new MooiPlacemarkDto {
                Id = placemark.Id,
                Index = placemark.Cluster.Placemarks.IndexOf(placemark) + 1,
                Name = placemark.Name,
                Description = placemark.Description,
                Images = placemark.Images.ToArray(),
                Coordinates = placemark.Coordinates.Select(ConvertCoordinateToString).ToArray(),
                AttachedVenues = placemark.AttachedVenues.Select(x => _venueConverters[x.SourceType](x)).ToArray(),
                IconPath = placemark.IconPathIsOnWeb ? placemark.IconPath : ConvertToLocalFileUrl(placemark.IconPath),
                ThumbnailFilePath = ConvertToLocalFileUrl(placemark.ThumbnailMapFilePath),
                IsShape = placemark.IsShape,
                Distance = placemark.Distance
            };

            return pm;
        }

        private string ConvertToLocalFileUrl(string filePath)
        {
            if (filePath == null)
                return null;
            return "localfile://" + filePath.Replace('\\', '/');
        }

        private string ConvertCoordinateToString(GeoCoordinate coord)
        {
            return _formatter.Format(coord.Latitude, COORDINATE_VALUE_PRECISION) + ","
                + _formatter.Format(coord.Longitude, COORDINATE_VALUE_PRECISION);
        }

        private static VenueBaseDto CreateHereVenue(VenueBase venue)
        {
            var hereVenue = venue as HereVenue;

            if (hereVenue == null || hereVenue.IsUseless())
                return null;

            return new HereVenueDto
            {
                SourceType = venue.SourceType.ToString(),
                Category = venue.Category,
                Title = venue.Title,
                Address = venue.Address,
                ContactPhone = venue.ContactPhone,
                Website = venue.Websites == null ? null : string.Join(", ", venue.Websites),
                OpeningHours = venue.OpeningHours,

                WikipediaContent = hereVenue.WikipediaContent
            };
        }

        private static VenueBaseDto CreateFoursquareVenue(VenueBase venue)
        {
            var foursquareVenue = venue as FoursquareVenue;

            if (foursquareVenue == null || foursquareVenue.IsUseless())
                return null;

            return new FoursquareVenueDto {
                SourceType = venue.SourceType.ToString(),
                Category = venue.Category,
                Title = venue.Title,
                Address = venue.Address,
                ContactPhone = venue.ContactPhone,
                Website = venue.Websites == null ? null : string.Join(", ", venue.Websites),
                OpeningHours = venue.OpeningHours,

                Rating = foursquareVenue.Rating,
                MaxRating = foursquareVenue.MaxRating,
                RatingColor = foursquareVenue.RatingColor,
                LikesCount = foursquareVenue.LikesCount,
                PriceLevel = foursquareVenue.PriceLevel.HasValue
                    ? string.Join(string.Empty, Enumerable.Range(1, foursquareVenue.PriceLevel.Value).Select(x => foursquareVenue.PriceCurrency))
                    : null,
                RemainingPriceLevel = foursquareVenue.PriceLevel.HasValue
                    ? string.Join(string.Empty, Enumerable.Range(1, foursquareVenue.PriceMaxLevel - foursquareVenue.PriceLevel.Value).Select(x => foursquareVenue.PriceCurrency))
                    : null,
                PhotoUrls = foursquareVenue.PhotoUrls,
                Tags = foursquareVenue.Tags,
                Tips = foursquareVenue.Tips?.Select(x => new FoursquareVenueTipDto {
                    Message = x.Message,
                    Likes = x.Likes,
                    AgreeCount = x.AgreeCount,
                    DisagreeCount = x.DisagreeCount
                }).ToArray()
            };
        }
    }
}
