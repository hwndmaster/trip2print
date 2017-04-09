using System;

namespace TripToPrint.Core
{
    public interface IGoogleMyMapAdapter
    {
        Uri GetKmlDownloadUrl(Uri mymapUrl);
        bool DoesLookLikeMyMapsUrl(string url);
    }

    public class GoogleMyMapAdapter : IGoogleMyMapAdapter
    {
        public Uri GetKmlDownloadUrl(Uri mymapUrl)
        {
            var authority = mymapUrl.GetLeftPart(UriPartial.Authority);
            var urlParams = System.Web.HttpUtility.ParseQueryString(mymapUrl.Query);
            var path = "/maps/d/kml?";
            return new Uri($"{authority}{path}mid={urlParams["mid"]}");
        }

        public bool DoesLookLikeMyMapsUrl(string url)
        {
            Uri parsedUri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out parsedUri))
            {
                return false;
            }

            var urlParams = System.Web.HttpUtility.ParseQueryString(parsedUri.Query);

            return urlParams["mid"] != null;
        }
    }
}
