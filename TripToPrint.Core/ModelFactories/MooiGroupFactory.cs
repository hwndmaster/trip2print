using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.ModelFactories
{
    public interface IMooiGroupFactory
    {
        List<MooiGroup> CreateList(KmlFolder folder, Dictionary<KmlPlacemark, DiscoveredPlace> discoveredPlacePerPlacemark, string reportTempPath);
    }

    public class MooiGroupFactory : IMooiGroupFactory
    {
        private const int MIN_GROUP_COUNT = 4;
        private const int MAX_GROUP_COUNT = 8;
        private const double MIN_DIST_TO_NEIGHBOR_IN_KM = 0.75d;
        private const double COEF_ROOM_OVER_MAX_DISTANCE = 1.5; // 50%

        private readonly IKmlCalculator _kmlCalculator;
        private readonly IResourceNameProvider _resourceName;
        private readonly IMooiPlacemarkFactory _mooiPlacemarkFactory;

        public MooiGroupFactory(IKmlCalculator kmlCalculator, IResourceNameProvider resourceName, IMooiPlacemarkFactory mooiPlacemarkFactory)
        {
            _kmlCalculator = kmlCalculator;
            _resourceName = resourceName;
            _mooiPlacemarkFactory = mooiPlacemarkFactory;
        }

        public List<MooiGroup> CreateList(KmlFolder folder, Dictionary<KmlPlacemark, DiscoveredPlace> discoveredPlacePerPlacemark
            , string reportTempPath)
        {
            var groups = new List<MooiGroup>();

            var placemarksConverted = folder.Placemarks
                .Select(x => _mooiPlacemarkFactory.Create(x,
                    discoveredPlacePerPlacemark?.ContainsKey(x) == true ? discoveredPlacePerPlacemark[x] : null,
                    reportTempPath))
                .ToList();

            if (placemarksConverted.Count <= MIN_GROUP_COUNT)
            {
                return CreateSingleGroup(placemarksConverted, reportTempPath);
            }

            if (folder.ContainsRoute && _kmlCalculator.CompleteFolderIsRoute(folder))
            {
                return CreateSingleGroup(placemarksConverted, reportTempPath);
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

                AppendPlacemarkToGroup(startingPoint.Placemark, currentGroup);

                // Add its closest neighbor to current group
                if (!groups.Any(g => g.Placemarks.Any(p => p == startingPoint.NeighborWithMinDistance.Placemark)))
                {
                    AppendPlacemarkToGroup(startingPoint.NeighborWithMinDistance.Placemark, currentGroup);
                }

                foreach (var pm in placemarksToProcess.Skip(1).ToList())
                {
                    if (currentGroup.Placemarks.Any(x => x == pm.Placemark
                        || x == pm.NeighborWithMinDistance.Placemark
                        || pm.Neighbors.Any(n => n.Placemark == x && pm.NeighborWithMinDistance.AllowedDistance > n.Distance)
                        ))
                    {
                        if (currentGroup.Placemarks.Count >= MIN_GROUP_COUNT)
                        {
                            var maxDistanceAmongAddedPlacemarks = placemarksWithNeighborsLookup[currentGroup.Placemarks[0]]
                                .Neighbors.Where(x => currentGroup.Placemarks.Any(y => y == x.Placemark))
                                .Select(x => x.AllowedDistance)
                                .Max();

                            if (pm.NeighborWithMinDistance.Distance > maxDistanceAmongAddedPlacemarks)
                            {
                                continue;
                            }
                        }

                        AppendPlacemarkToGroup(pm.Placemark, currentGroup);
                        placemarksToProcess.Remove(pm);
                    }

                    if (currentGroup.Placemarks.Count == MAX_GROUP_COUNT)
                    {
                        break;
                    }
                }

                currentGroup.OverviewMapFilePath = Path.Combine(reportTempPath, _resourceName.CreateFileNameForOverviewMap(currentGroup));
                currentGroup = new MooiGroup();
                groups.Add(currentGroup);
            }

            // Trim out the last group which is always empty
            groups = groups.Where(x => x.Placemarks.Count > 0).ToList();

            MergeGroups(groups);

            return groups;
        }

        public List<MooiGroup> CreateSingleGroup(List<MooiPlacemark> placemarks, string reportTempPath)
        {
            var group = new MooiGroup();
            foreach (var pm in placemarks)
            {
                AppendPlacemarkToGroup(pm, group);
            }
            group.OverviewMapFilePath = Path.Combine(reportTempPath, _resourceName.CreateFileNameForOverviewMap(group));

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

                if (groupForMerge.group.Placemarks.Count >= MIN_GROUP_COUNT)
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
                    AppendPlacemarkToGroup(pm, groupForMerge.group);
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

        private void AppendPlacemarkToGroup(MooiPlacemark placemark, MooiGroup group)
        {
            placemark.Group = group;
            group.Placemarks.Add(placemark);
        }
    }
}
