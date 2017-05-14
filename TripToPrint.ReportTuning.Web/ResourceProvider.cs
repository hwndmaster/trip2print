using System.IO;

namespace TripToPrint.ReportTuning.Web
{
    public static class ResourceProvider
    {
        public static Stream GetStream(string path)
        {
            var resourceName = typeof(ResourceProvider).Namespace + path.Replace('/', '.');
            return typeof(ResourceProvider).Assembly.GetManifestResourceStream(resourceName);
        }
    }
}
