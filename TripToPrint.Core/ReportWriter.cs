using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IReportWriter
    {
        Task<string> WriteReportAsync(MooiDocument document);
    }

    public class ReportWriter : IReportWriter
    {
        private readonly IKmlCalculator _kmlCalculator;
        private readonly IResourceNameProvider _resourceName;
        private readonly CultureAgnosticFormatter _formatter = new CultureAgnosticFormatter();

        private const int COORDINATE_VALUE_PRECISION = 6;
        private const int DISTANCE_IN_METERS_THRESHOLD = 2000;

        public ReportWriter(IKmlCalculator kmlCalculator, IResourceNameProvider resourceName)
        {
            _kmlCalculator = kmlCalculator;
            _resourceName = resourceName;
        }

        public async Task<string> WriteReportAsync(MooiDocument document)
        {
            return await Task.Run(() => this.RenderReport(document));
        }

        private string RenderReport(MooiDocument document)
        {
            var sb = new StringBuilder();

            sb.Append(@"<!DOCTYPE html><html xmlns=""http://www.w3.org/1999/xhtml""><head><meta charset=""utf-8"" />");
            sb.Append($"<title>{document.Title}</title>");
            sb.Append(@"<style>
                body { margin: 0; -webkit-print-color-adjust: exact; font-family: Arial }
                h3 { margin: 0; font-size: 12pt; }
                .doc-desc { font-style: italic; color: gray; }
                .ov { padding-top: 5px; overflow: hidden; }
                .ov-notfirst { page-break-before: always; }
                .ov .title { position: absolute; left: 8px; z-index: 2; margin-top: 8px; padding: 1px 6px; display: inline; background: white; border-radius: 8px; border: 1px solid #ccc; font-size: 10pt; }
                .ov img { width: 100%; }
                .pm-cols { overflow: hidden; }
                .pm-col { width: 49.9999%; float: left; }
                .pm { overflow: hidden; padding: 1px; page-break-inside: avoid; }
                .pm-col .pm { border: 1px solid #ccc; margin: 0 1px 2px 0; }
                .pm .title { color: black; font-weight: bold; font-size: 12pt; }
                .pm .header { font-family: 'Calibri Light'; }
                .pm .ix { position: relative; float: left; top: 106px; margin-left: -30px; background: #4189b3; border-radius: 10px; padding: 1px 6px; color: white; font-family: 'Consolas' }
                .pm-desc { font-size: 9.5pt; }
                .pm-xtra { font-size: 9pt; color: #444444; }
                .pm-xtra hr { margin: 5px 0; border: 0; border-top: 1px solid gray; }
                .pm-img img { max-width: 170px; max-height: 120px; float: left; margin-right: 4px; }
                .icon { position: relative; z-index: 5; float: left; }
                .pm-col .icon { width: 30px; }
                .dir .icon { width: 20px; padding-right: 2px; }
                .map { max-height: 130px; position: relative; vertical-align: top; left: -30px; float: left; margin-right: -26px; }
                .coord { color: gray; font-size: 9pt; font-weight: bold; }
                .coord a { color: gray; }

                @media print {
                    /* TODO: Temporary solution while scaling in CEF v57 cannot be adjusted */
                    .pm { zoom: 0.8; }
                }
                </style>");
            sb.Append(@"</head><body>");

            sb.AppendLine($"<h3>{document.Title}</h3>");
            if (!string.IsNullOrEmpty(document.Description))
            {
                sb.Append($"<p class='doc-desc'>{document.Description}</p>");
            }

            foreach (var folder in document.Sections)
            {
                RenderSection(folder, sb, folder == document.Sections[0]);
            }

            sb.Append(@"</body></html>");

            return sb.ToString();
        }

        private void RenderSection(MooiSection section, StringBuilder sb, bool first)
        {
            sb.AppendLine("<div class='folder'>");
            foreach (var group in section.Groups)
            {
                RenderGroup(group, sb, first);
                first = false;
            }
            sb.AppendLine("</div>");
        }

        private void RenderGroup(MooiGroup group, StringBuilder sb, bool first)
        {
            sb.Append($"<div class='ov {(first ? string.Empty : "ov-notfirst")}'>");
            sb.AppendLine($"<h4 class='title'>{group.Section.Name}</h3>");
            sb.Append($"<img src='{_resourceName.CreateFileNameForOverviewMap(@group)}' />");
            sb.Append("</div>");

            if (group.Type == GroupType.Points)
            {
                var meaningSizeOfGroup = group.Placemarks.Count + group.Placemarks
                    .Select(x => Math.Max(1, CountOfImagesInPlacemark(x)) - 1)
                    .Sum();

                sb.Append("<div class='pm-cols'>");
                sb.Append("<div class='pm-col'>");
                var i = 0;
                foreach (var placemark in group.Placemarks)
                {
                    if (i >= meaningSizeOfGroup / 2)
                    {
                        sb.Append("</div><div class='pm-col'>");
                        i = int.MinValue;
                    }

                    RenderPlacemark(group, placemark, sb);
                    i += Math.Max(1, CountOfImagesInPlacemark(placemark));
                }
                sb.Append("</div>");
                sb.Append("</div>");
            }
            else if (group.Type == GroupType.Routes)
            {
                sb.Append("<div class='dir'>");
                foreach (var placemark in group.Placemarks)
                {
                    RenderPlacemark(group, placemark, sb);
                }
                sb.Append("</div>");
            }
            else
            {
                throw new NotSupportedException($"Group type is not supported: {group.Type}");
            }
        }

        private void RenderPlacemark(MooiGroup group, MooiPlacemark placemark, StringBuilder sb)
        {
            var coordinate = _formatter.Format(placemark.PrimaryCoordinate.Latitude, COORDINATE_VALUE_PRECISION) + ","
                + _formatter.Format(placemark.PrimaryCoordinate.Longitude, COORDINATE_VALUE_PRECISION);
            var iconPath = placemark.IconPathIsOnWeb
                ? StringHelper.MakeUrlStringSafeForFileName(placemark.IconPath)
                : $"{placemark.IconPath}";
            sb.Append("<div class='pm'>");
            sb.Append($"<img class='icon' src='{iconPath}' />");

            if (group.Type == GroupType.Points)
            {
                sb.Append($"<img class='map' src='{_resourceName.CreateFileNameForPlacemarkThumbnail(placemark)}' />");
                sb.Append($"<div class='ix'>{group.Placemarks.IndexOf(placemark) + 1}</div>");
            }

            sb.Append($"<div class='header'>");
            sb.Append($"<span class='coord'>(<a href='http://maps.google.com/?ll={coordinate}'>{coordinate}</a>)</span> <span class='title'>{placemark.Name}</span>");
            if (_kmlCalculator.PlacemarkIsShape(placemark))
            {
                var distanceInMeters = _kmlCalculator.CalculateRouteDistanceInMeters(placemark);
                sb.Append(" <span class='dist'>(");
                if (distanceInMeters < DISTANCE_IN_METERS_THRESHOLD)
                {
                    sb.Append($"{distanceInMeters:#,##0} m");
                }
                else
                {
                    var distanceInKm = distanceInMeters / 1000;
                    sb.Append($"{distanceInKm:#,##0} km");
                }
                sb.Append(")</span>");
            }
            sb.Append($"</div>");

            if (!string.IsNullOrEmpty(placemark.Description))
            {
                sb.Append($"<div class='pm-desc'>{placemark.Description}</div>");
            }
            if (!string.IsNullOrEmpty(placemark.ImagesContent))
            {
                sb.Append($"<div class='pm-img'>{placemark.ImagesContent}</div>");
            }
            if (placemark.DiscoveredData != null && !placemark.DiscoveredData.IsUseless())
            {
                string sep = null;
                sb.Append("<div class='pm-xtra'><hr />");
                if (!string.IsNullOrEmpty(placemark.DiscoveredData.Address))
                {
                    sb.Append(placemark.DiscoveredData.Address);
                    sep = " | ";
                }
                if (!string.IsNullOrEmpty(placemark.DiscoveredData.ContactPhone))
                {
                    sb.Append($"{sep}{placemark.DiscoveredData.ContactPhone}");
                    sep = " | ";
                }
                if (!string.IsNullOrEmpty(placemark.DiscoveredData.Website))
                {
                    sb.Append($"{sep}{placemark.DiscoveredData.Website}");
                }
                if (!string.IsNullOrEmpty(placemark.DiscoveredData.OpeningHours))
                {
                    // TODO: Here уже даёт строку "Opening Hours" на нужном языке.
                    sb.Append($"{(sep == null ? null : "<br/>")}Opening hours: {placemark.DiscoveredData.OpeningHours}");
                }
                if (sep != null)
                {
                    sb.Append("<br/>");
                }
                if (!string.IsNullOrEmpty(placemark.DiscoveredData.WikipediaContent))
                {
                    sb.Append($"Wikipedia: {placemark.DiscoveredData.WikipediaContent}");
                }
                sb.Append("</div>");
            }

            sb.AppendLine("</div>");
        }

        private int CountOfImagesInPlacemark(MooiPlacemark placemark)
        {
            if (string.IsNullOrEmpty(placemark.ImagesContent))
            {
                return 0;
            }

            return Regex.Matches(placemark.ImagesContent, "<img").Count;
        }
    }
}
