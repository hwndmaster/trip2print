using System;
using System.IO;
using System.Net;
using CefSharp;

namespace TripToPrint.Chromium
{
    public class FileResourceHandler : ResourceHandler
    {
        public override bool ProcessRequestAsync(IRequest request, ICallback callback)
        {
            var uri = new Uri(request.Url);

            this.Stream = File.OpenRead(uri.LocalPath);
            if (this.Stream == null)
            {
                throw new NullReferenceException($"Local file was not found: {uri.AbsolutePath}");
            }

            this.StatusCode = (int)HttpStatusCode.OK;
            this.ResponseLength = Stream.Length;
            this.MimeType = GetMimeType(Path.GetExtension(uri.AbsolutePath));

            callback.Continue();

            return true;
        }
    }
}
