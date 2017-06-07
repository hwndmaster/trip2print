using System.Collections.Generic;
using System.Linq;
using TripToPrint.Core.Models;
using TripToPrint.Core.Properties;

namespace TripToPrint.Core.ModelFactories
{
    public interface IMooiDocumentFactory
    {
        MooiDocument Create(KmlDocument kmlDocument, List<DiscoveredPlace> discoveredPlaces, string reportTempPath);
    }

    internal class MooiDocumentFactory : IMooiDocumentFactory
    {
        private readonly IMooiGroupFactory _mooiGroupFactory;

        public MooiDocumentFactory(IMooiGroupFactory mooiGroupFactory)
        {
            _mooiGroupFactory = mooiGroupFactory;
        }

        public MooiDocument Create(KmlDocument kmlDocument, List<DiscoveredPlace> discoveredPlaces, string reportTempPath)
        {
            var model = new MooiDocument
            {
                Title = kmlDocument.Title,
                Description = kmlDocument.Description
            };

            var foldersWithPlacemarks = kmlDocument.Folders.Where(x => x.Placemarks.Any()).ToList();

            AppendExploredPlaces(discoveredPlaces, foldersWithPlacemarks);

            discoveredPlaces = discoveredPlaces?.Where(x => x.IsForPlacemark).ToList();

            foreach (var folder in foldersWithPlacemarks)
            {
                var section = new MooiSection
                {
                    Document = model,
                    Name = folder.Name
                };
                model.Sections.Add(section);

                ExtractGroupsFromFolderIntoSection(folder, section, discoveredPlaces, reportTempPath);
            }

            return model;
        }

        private void AppendExploredPlaces(IEnumerable<DiscoveredPlace> discoveredPlaces, ICollection<KmlFolder> folders)
        {
            var exploredPlaces = discoveredPlaces?.Where(x => !x.IsForPlacemark).ToList();
            if (exploredPlaces == null || exploredPlaces.Count == 0)
            {
                return;
            }

            var folder = new KmlFolder(Resources.Kml_Folder_Explored);
            foreach (var place in exploredPlaces)
            {
                var placemark = new KmlPlacemark
                {
                    Name = place.Venue.Title,
                    Coordinates = new[] { place.Venue.Coordinate },
                    IconPath = place.Venue.IconUrl?.ToString()
                };
                place.AttachedToPlacemark = placemark;
                folder.Placemarks.Add(placemark);
            }
            folders.Add(folder);
        }

        private void ExtractGroupsFromFolderIntoSection(KmlFolder folder, MooiSection section, List<DiscoveredPlace> discoveredPlaces, string reportTempPath)
        {
            var groups = _mooiGroupFactory.CreateList(folder, discoveredPlaces, reportTempPath);
            groups.ForEach(x => x.Section = section);
            section.Groups.AddRange(groups);
        }
    }
}
