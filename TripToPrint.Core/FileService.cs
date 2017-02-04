using System.IO;

namespace TripToPrint.Core
{
    public interface IFileService
    {
        bool Exists(string path);
    }

    public class FileService : IFileService
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}
