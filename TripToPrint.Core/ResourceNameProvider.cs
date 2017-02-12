﻿using System;

using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IResourceNameProvider
    {
        string CreateFileNameForOverviewMap(MooiGroup group);
        string CreateFileNameForPlacemarkThumbnail(MooiPlacemark placemark);
        string CreateTempFolderName(string suffix = null);
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

        public string CreateTempFolderName(string suffix = null)
        {
            suffix = suffix ?? Guid.NewGuid().ToString();

            return $"Trip2Print_{suffix}";
        }

        public string GetDefaultHtmlReportName()
        {
            return "index.html";
        }
    }
}
