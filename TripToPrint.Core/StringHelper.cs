using System.IO;
using System.Text.RegularExpressions;

namespace TripToPrint.Core
{
    public static class StringHelper
    {
        public static string MakeUrlStringSafeForFileName(string url, bool preserveExtension = true)
        {
            url = Regex.Replace(url, "https?://", string.Empty);

            var extension = string.Empty;
            if (preserveExtension)
            {
                extension = Path.GetExtension(url);
                url = url.Substring(0, url.Length - extension.Length);
            }

            url = Regex.Replace(url, @"[^\w]", string.Empty) + extension;

            return url;
        }
    }
}
