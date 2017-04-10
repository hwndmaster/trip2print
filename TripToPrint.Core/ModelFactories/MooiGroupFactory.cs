using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.ModelFactories
{
    public interface IMooiGroupFactory
    {
        List<MooiGroup> CreateList(KmlFolder folder, Dictionary<KmlPlacemark, DiscoveredPlace> discoveredPlacePerPlacemark);
    }

    public class MooiGroupFactory : IMooiGroupFactory
    {
        private const int MIN_GROUP_COUNT = 4;
        private const int MAX_GROUP_COUNT = 8;
        private const double MIN_DIST_TO_NEIGHBOR_IN_KM = 0.75d;
        private const double COEF_ROOM_OVER_MAX_DISTANCE = 1.5; // 50%

        private readonly IKmlCalculator _kmlCalculator;

        public MooiGroupFactory(IKmlCalculator kmlCalculator)
        {
            _kmlCalculator = kmlCalculator;
        }

        public List<MooiGroup> CreateList(KmlFolder folder, Dictionary<KmlPlacemark, DiscoveredPlace> discoveredPlacePerPlacemark)
        {
            var groups = new List<MooiGroup>();

            var placemarksConverted = folder.Placemarks
                .Select(x => ConvertKmlPlacemarkToMooiPlacemark(x,
                discoveredPlacePerPlacemark?.ContainsKey(x) == true ? discoveredPlacePerPlacemark[x] : null))
                .ToList();

            if (placemarksConverted.Count <= MIN_GROUP_COUNT)
            {
                return CreateSingleGroup(placemarksConverted);
            }

            if (folder.ContainsRoute && _kmlCalculator.CompleteFolderIsRoute(folder))
            {
                return CreateSingleGroup(placemarksConverted);
            }

            // TODO: Add support of lines within a folder which are not 'routes'
            placemarksConverted = placemarksConverted.Where(x => x.Coordinates.Length == 1).ToList();
            // ^^^

            var placemarksWithNeighbors = GetPlacemarksWithNeighbors(placemarksConverted).ToList();
            var placemarksWithNeighborsLookup = placemarksWithNeighbors.ToDictionary(x => x.Placemark);

            var currentGroup = new MooiGroup();
            groups.Add(currentGroup);

            var placemarksToProcess = placemarksWithNeighbors.ToList();
            while (placemarksToProcess.Any())
            {
                var startingPoint = placemarksToProcess[0];
                placemarksToProcess.RemoveAt(0);

                // Skip if the placemark has been added to any group before
                if (groups.Any(g => g.Placemarks.Any(p => p == startingPoint.Placemark)))
                {
                    continue;
                }

                currentGroup.Placemarks.Add(startingPoint.Placemark);

                // Add its closest neighbor to current group
                if (!groups.Any(g => g.Placemarks.Any(p => p == startingPoint.NeighborWithMinDistance.Placemark)))
                {
                    currentGroup.Placemarks.Add(startingPoint.NeighborWithMinDistance.Placemark);
                }

                foreach (var pm in placemarksToProcess.Skip(1).ToList())
                {
                    if (currentGroup.Placemarks.Any(x => x == pm.Placemark || x == pm.NeighborWithMinDistance.Placemark))
                    {
                        if (currentGroup.Placemarks.Count >= MIN_GROUP_COUNT)
                        {
                            /*var linkToFirst = placemarksLookup[currentGroup.Placemarks[0]]
                                .neighbors.First(x => x.placemark == pm.placemark);*/
                            var maxDistanceAmongAddedPlacemarks = placemarksWithNeighborsLookup[currentGroup.Placemarks[0]]
                                .Neighbors.Where(x => currentGroup.Placemarks.Any(y => y == x.Placemark))
                                .Select(x => x.AllowedDistance)
                                .Max();

                            if (pm.NeighborWithMinDistance.Distance > maxDistanceAmongAddedPlacemarks) // * COEF_ROOM_OVER_MAX_DISTANCE)
                            {
                                continue;
                            }
                        }

                        currentGroup.Placemarks.Add(pm.Placemark);
                        placemarksToProcess.Remove(pm);
                    }

                    if (currentGroup.Placemarks.Count == MAX_GROUP_COUNT)
                    {
                        break;
                    }
                }

                currentGroup = new MooiGroup();
                groups.Add(currentGroup);

                //placemarksToProcess = placemarksToProcess.Where(x => !currentGroup.Placemarks.Any(y => y == x.placemark || y == x.minDist.placemark)).ToList();
            }

            // Trim out the last group which is always empty
            groups = groups.Where(x => x.Placemarks.Count > 0).ToList();

            MergeGroups(groups);

            return groups;
        }

        public MooiPlacemark ConvertKmlPlacemarkToMooiPlacemark(KmlPlacemark kmlPlacemark, DiscoveredPlace discoveredPlace)
        {
            string imagesContent;
            var description = ExtractImagesFromContent(kmlPlacemark.Description, out imagesContent);
            description = FilterContent(description);

            return new MooiPlacemark
            {
                Name = kmlPlacemark.Name,
                Description = description,
                DiscoveredData = discoveredPlace,
                ImagesContent = imagesContent,
                Coordinates = kmlPlacemark.Coordinates,
                IconPath = kmlPlacemark.IconPath
            };
        }

        public List<MooiGroup> CreateSingleGroup(List<MooiPlacemark> placemarks)
        {
            var group = new MooiGroup();
            foreach (var pm in placemarks)
            {
                pm.Group = group;
                group.Placemarks.Add(pm);
            }

            return new List<MooiGroup> { group };
        }

        public virtual IEnumerable<PlacemarkWithNeighbors> GetPlacemarksWithNeighbors(IList<MooiPlacemark> placemarks)
        {
            return from placemark in placemarks
                   let neighbors = from neighbor in placemarks.Except(new[] { placemark })
                                   let dist = placemark.PrimaryCoordinate.GetDistanceTo(neighbor.PrimaryCoordinate)
                                   let allowedDistCoef = Math.Exp(1 / Math.Max(MIN_DIST_TO_NEIGHBOR_IN_KM, dist / 1000d))
                                   orderby dist
                                   select new PlacemarkNeighbor
                                   {
                                       Placemark = neighbor,
                                       Distance = dist,
                                       AllowedDistance = Math.Max(MIN_DIST_TO_NEIGHBOR_IN_KM * 1000, allowedDistCoef * dist)
                                   }
                   let minDist = neighbors.First()
                   orderby minDist.Distance
                   select new PlacemarkWithNeighbors
                   {
                       Placemark = placemark,
                       Neighbors = neighbors,
                       NeighborWithMinDistance = minDist
                   };
        }

        public void MergeGroups(List<MooiGroup> groups)
        {
            var groupsConsideredAsFine = new List<MooiGroup>();

            while (true)
            {
                var groupToMerge = groups
                    .Except(groupsConsideredAsFine)
                    .FirstOrDefault(x => x.Placemarks.Count < MIN_GROUP_COUNT);
                if (groupToMerge == null)
                    break;

                var groupForMerge =
                    (from forMergeCandidate in groups.Except(new[] { groupToMerge })
                     where forMergeCandidate.Placemarks.Count + groupToMerge.Placemarks.Count <= MAX_GROUP_COUNT
                     let minDist = CalculateDistances(groupToMerge, forMergeCandidate).Min()
                     let placemarksInGroup = (double) forMergeCandidate.Placemarks.Count
                     orderby placemarksInGroup < MIN_GROUP_COUNT
                         ? minDist * (placemarksInGroup / MIN_GROUP_COUNT)
                         : minDist
                     select new { @group = forMergeCandidate, minDist }).FirstOrDefault();

                if (groupForMerge == null)
                {
                    break;
                }

                if (groupToMerge.Placemarks.Count == MIN_GROUP_COUNT - 1
                    && groupForMerge.group.Placemarks.Count >= MIN_GROUP_COUNT)
                {
                    var dist1 = CalculateDistances(groupForMerge.group, groupForMerge.group).Max();

                    if (dist1 * COEF_ROOM_OVER_MAX_DISTANCE < groupForMerge.minDist)
                    {
                        groupsConsideredAsFine.Add(groupToMerge);
                        continue;
                    }
                }

                foreach (var pm in groupToMerge.Placemarks)
                {
                    pm.Group = groupForMerge.group;
                    groupForMerge.group.Placemarks.Add(pm);
                }

                groups.Remove(groupToMerge);
            }
        }

        private IEnumerable<double> CalculateDistances(MooiGroup group1, MooiGroup group2)
        {
            return from pm1 in group2.Placemarks
                   from pm2 in group1.Placemarks
                   where pm1 != pm2
                   select DistanceBetweenPlacemarks(pm1, pm2);
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

            // Remove br's which go after images
            //content = Regex.Replace(content, @"/><br>", "/>");

            // Wrap links with html <a> tags
            content = Regex.Replace(content,
                @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                "<a href='$1'>$1</a>");

            return content;
        }

        private string ExtractImagesFromContent(string content, out string imagesContent)
        {
            if (string.IsNullOrEmpty(content))
            {
                imagesContent = null;
                return content;
            }

            var foundImages = new List<string>();
            content = Regex.Replace(content, @"<img.+?>", m => {
                // Remove image sizes
                var imageFiltered = Regex.Replace(m.Value, @"\s*(height|width)=['""](\d+|auto)['""]", "");
                foundImages.Add(imageFiltered);
                return string.Empty;
            });

            imagesContent = string.Join(string.Empty, foundImages);

            return content;
        }

        private double DistanceBetweenPlacemarks(MooiPlacemark placemark1, MooiPlacemark placemark2)
        {
            return placemark1.PrimaryCoordinate.GetDistanceTo(placemark2.PrimaryCoordinate);
        }

        public class PlacemarkNeighbor
        {
            public MooiPlacemark Placemark { get; set; }
            public double Distance { get; set; }
            public double AllowedDistance { get; set; }
        }

        public class PlacemarkWithNeighbors
        {
            public MooiPlacemark Placemark { get; set; }
            public IEnumerable<PlacemarkNeighbor> Neighbors { get; set; }
            public PlacemarkNeighbor NeighborWithMinDistance { get; set; }
        }
    }
}
