using System.IO;
using System.Text.RegularExpressions;

namespace TripToPrint.Core
{
    public static class StringHelper
    {
        public static string MakeStringSafeForFileName(string value, bool preserveExtension = true)
        {
            value = Regex.Replace(value, "https?://", string.Empty);

            var extension = string.Empty;
            if (preserveExtension)
            {
                extension = Path.GetExtension(value);
                value = value.Substring(0, value.Length - extension.Length);
            }

            value = Regex.Replace(value, @"[^\w]", string.Empty) + extension;

            return value;
        }
    }
}
