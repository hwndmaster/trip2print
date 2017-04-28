using System.Diagnostics.CodeAnalysis;

namespace TripToPrint.Core
{
    public interface IZipService
    {
        IZipFileWrapper Open(string zipFileName);
    }

    [ExcludeFromCodeCoverage]
    public class ZipService : IZipService
    {
        public IZipFileWrapper Open(string zipFileName) => new ZipFileWrapper(zipFileName);
    }
}
