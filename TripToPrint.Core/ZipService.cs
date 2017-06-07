using System.Diagnostics.CodeAnalysis;

namespace TripToPrint.Core
{
    public interface IZipService
    {
        IZipFileWrapper Open(string zipFileName);
    }

    [ExcludeFromCodeCoverage]
    internal class ZipService : IZipService
    {
        public IZipFileWrapper Open(string zipFileName) => new ZipFileWrapper(zipFileName);
    }
}
