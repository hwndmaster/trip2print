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
        Task<string> WriteReportAsync(MooiDocument document, string resourcesPath);
    }

    public class ReportWriter : IReportWriter
    {
        public async Task<string> WriteReportAsync(MooiDocument document, string resourcesPath)
        {
            return await Task.Run(() => this.RenderReport(document, resourcesPath));
        }

        private string RenderReport(MooiDocument document, string resourcesPath)
        {
            var sb = new StringBuilder();

            sb.Append(@"<!DOCTYPE html><html xmlns=""http://www.w3.org/1999/xhtml""><head><meta charset=""utf-8"" />");
            sb.Append($"<title>{document.Title}</title>");
            sb.Append(@"<style>
                body { margin: 0; }
                h3 { margin: 0; }
                .doc-desc { font-style: italic; color: gray; }
                .ov { padding-top: 5px; overflow: hidden; }
                .ov-notfirst { page-break-before: always; }
                .ov .title { position: absolute; left: 8px; z-index: 2; margin-top: 8px; padding: 1px 6px; display: inline; background: white; border-radius: 8px; border: 1px solid #ccc; }
                .pm-cols { overflow: hidden; }
                .pm-col { width: 49.9999%; float: left; }
                .pm { border: 1px solid #ccc; overflow: hidden; margin: 0 1px 2px 0; padding: 1px; page-break-inside: avoid; }
                .pm h5, .pm h4 { margin: 0; }
                .pm .title { color: black; font-weight: bold; }
                .pm .ix { position: relative; float: left; top: 126px; margin-left: -30px; background: #4189b3; border-radius: 10px; padding: 1px 6px; color: white; font-family: 'Consolas' }
                .pm-desc { font-size: 9pt; }
                .pm-desc img { max-width: 200px; max-height: 150px; float: left; margin-right: 4px; }
                .icon { width: 30px; position: relative; z-index: 5; float: left; }
                .map { max-height: 150px; position: relative; vertical-align: top; left: -30px; float: left; margin-right: -26px; }
                .coord { color: gray; font-size: 9pt; font-weight: bold; }
                </style>");
            sb.Append(@"</head><body>");

            sb.AppendLine($"<h3>{document.Title}</h3>");
            if (!string.IsNullOrEmpty(document.Description))
            {
                sb.Append($"<p class='doc-desc'>{document.Description}</p>");
            }

            foreach (var folder in document.Sections)
            {
                RenderSection(folder, sb, resourcesPath, folder == document.Sections[0]);
            }

            sb.Append(@"</body></html>");

            return sb.ToString();
        }

        private void RenderSection(MooiSection section, StringBuilder sb, string resourcesPath, bool first)
        {
            sb.AppendLine("<div class='folder'>");
            foreach (var group in section.Groups)
            {
                RenderGroup(group, sb, resourcesPath, first);
                first = false;
            }
            sb.AppendLine("</div>");
        }

        private void RenderGroup(MooiGroup group, StringBuilder sb, string resourcesPath, bool first)
        {
            sb.Append($"<div class='ov {(first ? string.Empty : "ov-notfirst")}'>");
            sb.AppendLine($"<h4 class='title'>{group.Section.Name}</h3>");
            sb.Append($"<img src='file:///{resourcesPath}/{@group.OverviewMapFileName}' />");
            sb.Append("</div>");

            var meaningSizeOfGroup = group.Placemarks.Count + group.Placemarks
                .Select(x => Math.Max(1, CountOfImagesInDescription(x)) - 1)
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

                RenderPlacemark(group, placemark, sb, resourcesPath);
                i += Math.Max(1, CountOfImagesInDescription(placemark));
            }
            sb.Append("</div>");
            sb.Append("</div>");
        }

        private void RenderPlacemark(MooiGroup group, MooiPlacemark placemark, StringBuilder sb, string resourcesPath)
        {
            var coordinate = placemark.Coordinate.Latitude.ToString("0.######") + ","
                + placemark.Coordinate.Longitude.ToString("0.######");

            sb.Append("<div class='pm'>");
            sb.Append($"<img class='icon' src='file:///{resourcesPath}/{placemark.IconPath}' />");
            sb.Append($"<img class='map' src='file:///{resourcesPath}/{placemark.Id}.jpg' />");
            sb.Append($"<div class='ix'>{group.Placemarks.IndexOf(placemark) + 1}</div>");
            sb.Append($"<div><span class='coord'>(<a href='http://maps.google.com/?ll={coordinate}'>{coordinate}</a>)</span> <span class='title'>{placemark.Name}</span></div>");
            if (!string.IsNullOrEmpty(placemark.Description))
                sb.Append($"<div class='pm-desc'>{placemark.Description}</div>");
            sb.AppendLine("</div>");
        }

        private int CountOfImagesInDescription(MooiPlacemark placemark)
        {
            if (string.IsNullOrEmpty(placemark.Description))
                return 0;

            return Regex.Matches(placemark.Description, "<img").Count;
        }
    }
}
