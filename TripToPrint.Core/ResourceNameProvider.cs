using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IResourceNameProvider
    {
        string CreateFileNameForOverviewMap(MooiGroup group);
        string CreateFileNameForPlacemarkThumbnail(MooiPlacemark placemark);
        string GetDefaultHtmlReportName();
    }

    public class ResourceNameProvider : IResourceNameProvider
    {
        public string CreateFileNameForOverviewMap(MooiGroup group)
        {
            return $"overview-{group.Id}.jpg";
        }

        public string CreateFileNameForPlacemarkThumbnail(MooiPlacemark placemark)
        {
            return $"{placemark.Id}.jpg";
        }

        public string GetDefaultHtmlReportName()
        {
            return "index.html";
        }
    }
}
