using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TripToPrint.Core.Models;
using TripToPrint.Core.ProgressTracking;

namespace TripToPrint.Core
{
    public interface IDiscoveringService
    {
        Task<List<DiscoveredPlace>> Discover(KmlDocument document, string language, IDiscoveringProgress progressTracker, CancellationToken cancellationToken);
    }

    public class DiscoveringService : IDiscoveringService
    {
        private readonly IHereAdapter _here;
        private readonly IFoursquareAdapter _foursquare;
        private readonly IKmlCalculator _kmlCalculator;

        private const int DEGREE_OF_PARALLELISM_PER_SERVICE = 4;
        private const int EXPLORE_ON_PLACEMARKS_AFTER_METERS = 400;

        public DiscoveringService(IHereAdapter here, IFoursquareAdapter foursquare, IKmlCalculator kmlCalculator)
        {
            _here = here;
            _foursquare = foursquare;
            _kmlCalculator = kmlCalculator;
        }

        public async Task<List<DiscoveredPlace>> Discover(KmlDocument document, string language, IDiscoveringProgress progressTracker, CancellationToken cancellationToken)
        {
            var placemarks = document.Folders.Where(x => !x.ContainsRoute).SelectMany(x => x.Placemarks).ToList();

            const int loopsCount = 3;
            progressTracker.ReportNumberOfIterations(placemarks.Count * loopsCount);

            var hereTask = DiscoverThroughHere(placemarks, language, progressTracker, cancellationToken);
            var foursquareTask = DiscoverThroughFoursquare(placemarks, language, progressTracker, cancellationToken);

            var result = await Task.WhenAll(hereTask, foursquareTask);

            progressTracker.ReportDone();

            return result.SelectMany(x => x).ToList();
        }

        private async Task<List<DiscoveredPlace>> DiscoverThroughHere(IEnumerable<KmlPlacemark> placemarks, string language, IDiscoveringProgress progressTracker, CancellationToken cancellationToken)
        {
            var result = new ConcurrentBag<DiscoveredPlace>();
            
            await placemarks.ForEachAsync(DEGREE_OF_PARALLELISM_PER_SERVICE, async (placemark) => {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var venue = await _here.LookupMatchingVenue(placemark, language, cancellationToken);
                if (venue != null)
                {
                    result.Add(new DiscoveredPlace {
                        Venue = venue,
                        AttachedToPlacemark = placemark
                    });
                }

                progressTracker.ReportItemProcessed();
            });

            return result.ToList();
        }

        private async Task<List<DiscoveredPlace>> DiscoverThroughFoursquare(List<KmlPlacemark> placemarks, string language, IDiscoveringProgress progressTracker, CancellationToken cancellationToken)
        {
            var result = new ConcurrentBag<DiscoveredPlace>();

            await placemarks.ForEachAsync(DEGREE_OF_PARALLELISM_PER_SERVICE, async (placemark) => {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var venue = await _foursquare.LookupMatchingVenue(placemark, language, cancellationToken);
                if (venue != null)
                {
                    result.Add(new DiscoveredPlace
                    {
                        Venue = venue,
                        AttachedToPlacemark = placemark
                    });
                }

                progressTracker.ReportItemProcessed();
            });

            var placemarksToExplore = new List<KmlPlacemark> { placemarks[0] };
            foreach (var placemark in placemarks.Skip(1))
            {
                if (!placemarksToExplore.Any(p => _kmlCalculator.GetDistanceInMeters(p, placemark) < EXPLORE_ON_PLACEMARKS_AFTER_METERS))
                {
                    placemarksToExplore.Add(placemark);
                }
                else
                {
                    progressTracker.ReportItemProcessed();
                }
            }

            await placemarksToExplore.ForEachAsync(DEGREE_OF_PARALLELISM_PER_SERVICE, async (placemark) => {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var venues = await _foursquare.ExplorePopular(placemark, language, cancellationToken);
                if (venues != null)
                {
                    foreach (var venue in venues)
                    {
                        if (placemarks.Any(p => p.Name.Equals(venue.Title, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        result.Add(new DiscoveredPlace
                        {
                            Venue = venue
                        });
                    }
                }

                progressTracker.ReportItemProcessed();
            });

            return result.ToList();
        }
    }
}
