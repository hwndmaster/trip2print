using System.Collections.Generic;
using System.Device.Location;
using System.Linq;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Core.Tests.SpecflowDefinitions
{
    [Binding]
    public sealed class ClustersGenerationDefinition
    {
        private List<PlacemarkTableRow> _placemarkTableRows;

        [Given("I have these placemarks in my folder:")]
        public void GivenIHaveThesePlacemarksInMyFolder(Table table)
        {
            _placemarkTableRows = table.CreateSet<PlacemarkTableRow>().ToList();
        }

        [Then("these placemarks will be assigned to the following clusters:")]
        public void ThenThesePlacemarksWillBeAssignedToTheFollowingClusters(Table table)
        {
            var folder = new KmlFolder(_placemarkTableRows.Select(x => new KmlPlacemark {
                Coordinates = new[] { new GeoCoordinate(x.Latitude, x.Longitude) },
                Name = x.Name
            }));

            var kmlCalculator = new KmlCalculator();
            var resourceName = new ResourceNameProvider();
            var factory = new MooiClusterFactory(kmlCalculator, resourceName, new MooiPlacemarkFactory(kmlCalculator, resourceName));
            var clusters = factory.CreateList(folder, null, string.Empty);
            var result = clusters
                .SelectMany((cluster, clusterIndex)
                    => cluster.Placemarks.Select(placemark
                        => new { p = placemark, i = clusterIndex }))
                .Select(x => new PlacemarkInClusterTableRow {
                    Name = x.p.Name,
                    ClusterIndex = x.i
                });

            table.CompareToSet(result);
        }
    }
}
