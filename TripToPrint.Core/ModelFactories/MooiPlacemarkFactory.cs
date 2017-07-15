using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TripToPrint.Core.Models;
using TripToPrint.Core.Models.Venues;

namespace TripToPrint.Core.ModelFactories
{
    public interface IMooiPlacemarkFactory
    {
        MooiPlacemark Create(KmlPlacemark kmlPlacemark, IEnumerable<VenueBase> venues, string reportTempPath);
    }

    internal class MooiPlacemarkFactory : IMooiPlacemarkFactory
    {
        private readonly IKmlCalculator _kmlCalculator;
        private readonly IResourceNameProvider _resourceName;
        private readonly CultureAgnosticFormatter _formatter = new CultureAgnosticFormatter();

        private const int MAX_PHOTOS_TO_PICK_FROM_VENUE = 2;

        public MooiPlacemarkFactory(IKmlCalculator kmlCalculator, IResourceNameProvider resourceName)
        {
            _kmlCalculator = kmlCalculator;
            _resourceName = resourceName;
        }

        public MooiPlacemark Create(KmlPlacemark kmlPlacemark, IEnumerable<VenueBase> venues, string reportTempPath)
        {
            var descriptionAndImages = ExtractImagesFromContent(kmlPlacemark.Description);
            var description = FilterContent(descriptionAndImages.filteredContent);

            var placemark = new MooiPlacemark
            {
                Name = kmlPlacemark.Name,
                Description = description,
                AttachedVenues = venues?.ToArray(),
                Coordinates = kmlPlacemark.Coordinates,
                IconPath = kmlPlacemark.IconPath
            };

            if (descriptionAndImages.images != null)
            {
                placemark.Images.AddRange(descriptionAndImages.images);
            }

            if (placemark.IconPath != null && !placemark.IconPathIsOnWeb)
            {
                placemark.IconPath = Path.Combine(reportTempPath, placemark.IconPath);
            }

            placemark.ThumbnailMapFilePath = Path.Combine(reportTempPath,
                _resourceName.CreateFileNameForPlacemarkThumbnail(placemark));
            placemark.IsShape = _kmlCalculator.PlacemarkIsShape(placemark);

            if (placemark.IsShape)
            {
                var distanceInMeters = _kmlCalculator.CalculateRouteDistanceInMeters(placemark);
                placemark.Distance = _formatter.FormatDistance(distanceInMeters);
            }

            ExtendPlacemarkWithVenueData(placemark);

            return placemark;
        }

        public void ExtendPlacemarkWithVenueData(MooiPlacemark placemark)
        {
            if (placemark.AttachedVenues == null)
            {
                return;
            }

            foreach (var venue in placemark.AttachedVenues)
            {
                switch (venue.SourceType)
                {
                    case VenueSource.Here:
                        break;
                    case VenueSource.Foursquare:
                        var fsq = venue as FoursquareVenue;
                        if (fsq == null)
                            continue;
                        placemark.Images.AddRange(fsq.PhotoUrls.Take(MAX_PHOTOS_TO_PICK_FROM_VENUE));
                        break;
                    case VenueSource.Undefined:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private (string filteredContent, string[] images) ExtractImagesFromContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return (content, null);
            }

            var foundImages = new List<string>();
            content = Regex.Replace(content, @"<img.+?>", m => {
                var imageUrl = Regex.Match(m.Value, @"src=['""](?<url>.+?)['""]").Groups["url"].Value;
                foundImages.Add(imageUrl);
                return string.Empty;
            });

            return (content, foundImages.ToArray());
        }

        private string FilterContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            // Trim <br>'s at the beginning and at the end
            content = Regex.Replace(content, @"^(<br\s*/?>)+", string.Empty);
            content = Regex.Replace(content, @"(<br\s*/?>)+$", string.Empty);

            // Strip consecutive br's
            content = Regex.Replace(content, @"(<br\s*/?>){2,}", "<br>");

            // Wrap links with html <a> tags
            content = Regex.Replace(content,
                @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                "<a href='$1'>$1</a>");

            return content;
        }
    }
}
