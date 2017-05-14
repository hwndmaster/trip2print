using System;
using CefSharp;

namespace TripToPrint.Chromium
{
    public class SchemeHandlerFactory : ISchemeHandlerFactory
    {
        public const string T2P_SCHEME_NAME = "t2p";
        public const string FILE_SCHEME_NAME = "localfile";

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            var uri = new Uri(request.Url);
            
            if (uri.Scheme.Equals(T2P_SCHEME_NAME))
            {
                return new ReportTuningResourceHandler();
            }
            else if (uri.Scheme.Equals(FILE_SCHEME_NAME))
            {
                return new FileResourceHandler();
            }

            throw new NotSupportedException($"Scheme for provided URL is not supported: {uri}.");
        }
    }
}
