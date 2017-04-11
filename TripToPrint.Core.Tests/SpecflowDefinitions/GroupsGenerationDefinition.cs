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
    public sealed class GroupsGenerationDefinition
    {
        private List<PlacemarkTableRow> _placemarkTableRows;

        [Given("I have these placemarks in my folder:")]
        public void GivenIHaveThesePlacemarksInMyFolder(Table table)
        {
            _placemarkTableRows = table.CreateSet<PlacemarkTableRow>().ToList();
        }

        [Then("these placemarks will be assigned to the following groups:")]
        public void ThenThesePlacemarksWillBeAssignedToTheFollowingGroups(Table table)
        {
            var folder = new KmlFolder(_placemarkTableRows.Select(x => new KmlPlacemark {
                Coordinates = new[] { new GeoCoordinate(x.Latitude, x.Longitude) },
                Name = x.Name
            }));

            var factory = new MooiGroupFactory(new KmlCalculator());
            var groups = factory.CreateList(folder, null);
            var result = groups
                .SelectMany((@group, groupIndex)
                    => @group.Placemarks.Select(placemark
                        => new { p = placemark, i = groupIndex }))
                .Select(x => new PlacemarkInGroupTableRow {
                    Name = x.p.Name,
                    GroupIndex = x.i
                });

            table.CompareToSet(result);
        }
    }
}
