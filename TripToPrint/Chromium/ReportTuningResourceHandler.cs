using System;
using System.IO;
using System.Net;
using CefSharp;
using TripToPrint.ReportTuning.Web;

namespace TripToPrint.Chromium
{
    public class ReportTuningResourceHandler : ResourceHandler
    {
        public override bool ProcessRequestAsync(IRequest request, ICallback callback)
        {
            var uri = new Uri(request.Url);

            this.Stream = ResourceProvider.GetStream(uri.AbsolutePath);
            if (this.Stream == null)
            {
                throw new NullReferenceException($"Resource for path was not found: {uri.AbsolutePath}");
            }
            this.Stream.Position = 0;

            this.StatusCode = (int)HttpStatusCode.OK;
            this.ResponseLength = Stream.Length;
            this.MimeType = GetMimeType(Path.GetExtension(uri.AbsolutePath));

            callback.Continue();

            return true;
        }
    }
}
