using System;
using System.Device.Location;
using System.Linq;
using TripToPrint.Core;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;
using TripToPrint.ReportTuning.Dto;

namespace TripToPrint.Chromium
{
    public interface ITuningDtoFactory
    {
        MooiDocumentDto Create(MooiDocument document, string reportTempPath);
    }

    public class TuningDtoFactory : ITuningDtoFactory
    {
        private readonly IKmlCalculator _kmlCalculator;
        private readonly IMooiDocumentFactory _mooiDocumentFactory;
        private readonly CultureAgnosticFormatter _formatter = new CultureAgnosticFormatter();

        private const int COORDINATE_VALUE_PRECISION = 6;

        public TuningDtoFactory(IKmlCalculator kmlCalculator, IMooiDocumentFactory mooiDocumentFactory)
        {
            _kmlCalculator = kmlCalculator;
            _mooiDocumentFactory = mooiDocumentFactory;
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
                Groups = section.Groups.Select(CreateGroup).ToArray()
            };
        }

        private MooiGroupDto CreateGroup(MooiGroup group)
        {
            return new MooiGroupDto {
                Id = group.Id,
                IsRoute = group.Type == GroupType.Routes,
                OverviewMapFilePath = ConvertToLocalFileUrl(group.OverviewMapFilePath),
                Placemarks = group.Placemarks.Select(CreatePlacemark).ToArray()
            };
        }

        private MooiPlacemarkDto CreatePlacemark(MooiPlacemark placemark)
        {
            var pm = new MooiPlacemarkDto {
                Id = placemark.Id,
                Index = placemark.Group.Placemarks.IndexOf(placemark) + 1,
                Name = placemark.Name,
                Description = placemark.Description,
                Images = placemark.Images,
                Coordinates = placemark.Coordinates.Select(ConvertCoordinateToString).ToArray(),
                DiscoveredData = CreateDiscoveredPlace(placemark.DiscoveredData),
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
            return $"localfile://" + filePath.Replace('\\', '/');
        }

        private DiscoveredPlaceDto CreateDiscoveredPlace(DiscoveredPlace discoveredData)
        {
            if (discoveredData == null || discoveredData.IsUseless())
                return null;

            return new DiscoveredPlaceDto {
                Title = discoveredData.Title,
                Address = discoveredData.Address,
                ContactPhone = discoveredData.ContactPhone,
                Website = discoveredData.Website,
                AverageRating = discoveredData.AverageRating,
                OpeningHours = discoveredData.OpeningHours,
                WikipediaContent = discoveredData.WikipediaContent
            };
        }

        private string ConvertCoordinateToString(GeoCoordinate coord)
        {
            return _formatter.Format(coord.Latitude, COORDINATE_VALUE_PRECISION) + ","
                + _formatter.Format(coord.Longitude, COORDINATE_VALUE_PRECISION);
        }
    }
}
