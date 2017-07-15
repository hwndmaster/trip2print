using System;

using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IResourceNameProvider
    {
        string CreateFileNameForOverviewMap(MooiCluster cluster);
        string CreateFileNameForPlacemarkThumbnail(MooiPlacemark placemark);
        string CreateTempFolderName(string suffix = null);
        string GetDefaultHtmlReportName();
        string GetTempFolderPrefix();
    }

    internal class ResourceNameProvider : IResourceNameProvider
    {
        public string CreateFileNameForOverviewMap(MooiCluster cluster)
        {
            return $"overview-{cluster.Id}.jpg";
        }

        public string CreateFileNameForPlacemarkThumbnail(MooiPlacemark placemark)
        {
            return $"{placemark.Id}.jpg";
        }

        public string CreateTempFolderName(string suffix = null)
        {
            suffix = suffix ?? Guid.NewGuid().ToString();

            return $"{GetTempFolderPrefix()}{suffix}";
        }

        public string GetDefaultHtmlReportName()
        {
            return "index.html";
        }

        public string GetTempFolderPrefix()
        {
            return "Trip2Print_";
        }
    }
}
