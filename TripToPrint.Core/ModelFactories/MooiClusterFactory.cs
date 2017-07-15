using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.ModelFactories
{
    public interface IMooiClusterFactory
    {
        List<MooiCluster> CreateList(KmlFolder folder, List<DiscoveredPlace> discoveredPlaces, string reportTempPath);
    }

    internal class MooiClusterFactory : IMooiClusterFactory
    {
        private const int MIN_COUNT_PER_CLUSTER = 4;
        private const int MAX_COUNT_PER_CLUSTER = 8;
        private const double MIN_DIST_TO_NEIGHBOR_IN_KM = 0.75d;
        private const double COEF_ROOM_OVER_MAX_DISTANCE = 1.5; // 50%

        private readonly IKmlCalculator _kmlCalculator;
        private readonly IResourceNameProvider _resourceName;
        private readonly IMooiPlacemarkFactory _mooiPlacemarkFactory;

        public MooiClusterFactory(IKmlCalculator kmlCalculator, IResourceNameProvider resourceName, IMooiPlacemarkFactory mooiPlacemarkFactory)
        {
            _kmlCalculator = kmlCalculator;
            _resourceName = resourceName;
            _mooiPlacemarkFactory = mooiPlacemarkFactory;
        }

        public List<MooiCluster> CreateList(KmlFolder folder, List<DiscoveredPlace> discoveredPlaces, string reportTempPath)
        {
            var clusters = new List<MooiCluster>();

            var placemarksConverted = folder.Placemarks
                .Select(x => _mooiPlacemarkFactory.Create(x,
                    discoveredPlaces?.Where(dp => dp.AttachedToPlacemark == x).Select(dp => dp.Venue),
                    reportTempPath))
                .ToList();

            if (placemarksConverted.Count <= MIN_COUNT_PER_CLUSTER)
            {
                return CreateSingleCluster(placemarksConverted, reportTempPath);
            }

            if (folder.ContainsRoute && _kmlCalculator.CompleteFolderIsRoute(folder))
            {
                return CreateSingleCluster(placemarksConverted, reportTempPath);
            }

            // TODO: Add support of lines within a folder which are not 'routes'
            placemarksConverted = placemarksConverted.Where(x => x.Coordinates.Length == 1).ToList();
            // ^^^

            var placemarksWithNeighbors = GetPlacemarksWithNeighbors(placemarksConverted).ToList();
            var placemarksWithNeighborsLookup = placemarksWithNeighbors.ToDictionary(x => x.Placemark);

            var currentCluster = new MooiCluster();
            clusters.Add(currentCluster);

            var placemarksToProcess = placemarksWithNeighbors.ToList();
            while (placemarksToProcess.Any())
            {
                var startingPoint = placemarksToProcess[0];
                placemarksToProcess.RemoveAt(0);

                // Skip if the placemark has been added to any cluster before
                if (clusters.Any(g => g.Placemarks.Any(p => p == startingPoint.Placemark)))
                {
                    continue;
                }

                AppendPlacemarkToCluster(startingPoint.Placemark, currentCluster);

                // Add its closest neighbor to current cluster
                if (!clusters.Any(g => g.Placemarks.Any(p => p == startingPoint.NeighborWithMinDistance.Placemark)))
                {
                    AppendPlacemarkToCluster(startingPoint.NeighborWithMinDistance.Placemark, currentCluster);
                }

                foreach (var pm in placemarksToProcess.Skip(1).ToList())
                {
                    if (currentCluster.Placemarks.Any(x => x == pm.Placemark
                        || x == pm.NeighborWithMinDistance.Placemark
                        || pm.Neighbors.Any(n => n.Placemark == x && pm.NeighborWithMinDistance.AllowedDistance > n.Distance)
                        ))
                    {
                        if (currentCluster.Placemarks.Count >= MIN_COUNT_PER_CLUSTER)
                        {
                            var maxDistanceAmongAddedPlacemarks = placemarksWithNeighborsLookup[currentCluster.Placemarks[0]]
                                .Neighbors.Where(x => currentCluster.Placemarks.Any(y => y == x.Placemark))
                                .Select(x => x.AllowedDistance)
                                .Max();

                            if (pm.NeighborWithMinDistance.Distance > maxDistanceAmongAddedPlacemarks)
                            {
                                continue;
                            }
                        }

                        AppendPlacemarkToCluster(pm.Placemark, currentCluster);
                        placemarksToProcess.Remove(pm);
                    }

                    if (currentCluster.Placemarks.Count == MAX_COUNT_PER_CLUSTER)
                    {
                        break;
                    }
                }

                currentCluster.OverviewMapFilePath = Path.Combine(reportTempPath, _resourceName.CreateFileNameForOverviewMap(currentCluster));
                currentCluster = new MooiCluster();
                clusters.Add(currentCluster);
            }

            // Trim out the last cluster which is always empty
            clusters = clusters.Where(x => x.Placemarks.Count > 0).ToList();

            MergeClusters(clusters);

            return clusters;
        }

        public List<MooiCluster> CreateSingleCluster(List<MooiPlacemark> placemarks, string reportTempPath)
        {
            var cluster = new MooiCluster();
            foreach (var pm in placemarks)
            {
                AppendPlacemarkToCluster(pm, cluster);
            }
            cluster.OverviewMapFilePath = Path.Combine(reportTempPath, _resourceName.CreateFileNameForOverviewMap(cluster));

            return new List<MooiCluster> { cluster };
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

        public void MergeClusters(List<MooiCluster> clusters)
        {
            var clustersConsideredAsFine = new List<MooiCluster>();

            while (true)
            {
                var clusterToMerge = clusters
                    .Except(clustersConsideredAsFine)
                    .FirstOrDefault(x => x.Placemarks.Count < MIN_COUNT_PER_CLUSTER);
                if (clusterToMerge == null)
                    break;

                var clusterForMerge =
                    (from forMergeCandidate in clusters.Except(new[] { clusterToMerge })
                     where forMergeCandidate.Placemarks.Count + clusterToMerge.Placemarks.Count <= MAX_COUNT_PER_CLUSTER
                     let minDist = CalculateDistances(clusterToMerge, forMergeCandidate).Min()
                     let placemarksInCluster = (double) forMergeCandidate.Placemarks.Count
                     orderby placemarksInCluster < MIN_COUNT_PER_CLUSTER
                         ? minDist * (placemarksInCluster / MIN_COUNT_PER_CLUSTER)
                         : minDist
                     select new { cluster = forMergeCandidate, minDist }).FirstOrDefault();

                if (clusterForMerge == null)
                {
                    break;
                }

                if (clusterForMerge.cluster.Placemarks.Count >= MIN_COUNT_PER_CLUSTER)
                {
                    var dist1 = CalculateDistances(clusterForMerge.cluster, clusterForMerge.cluster).Max();

                    if (dist1 * COEF_ROOM_OVER_MAX_DISTANCE < clusterForMerge.minDist)
                    {
                        clustersConsideredAsFine.Add(clusterToMerge);
                        continue;
                    }
                }

                foreach (var pm in clusterToMerge.Placemarks)
                {
                    AppendPlacemarkToCluster(pm, clusterForMerge.cluster);
                }

                clusters.Remove(clusterToMerge);
            }
        }

        private IEnumerable<double> CalculateDistances(MooiCluster cluster1, MooiCluster cluster2)
        {
            return from pm1 in cluster2.Placemarks
                   from pm2 in cluster1.Placemarks
                   where pm1 != pm2
                   select _kmlCalculator.GetDistanceInMeters(pm1, pm2);
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

        private void AppendPlacemarkToCluster(MooiPlacemark placemark, MooiCluster cluster)
        {
            placemark.Cluster = cluster;
            cluster.Placemarks.Add(placemark);
        }
    }
}
