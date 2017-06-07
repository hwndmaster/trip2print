using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace TripToPrint.Core
{
    public interface IFileService
    {
        bool Exists(string path);
        void Delete(string filePath);
        void Move(string sourceFileName, string destFileName);
        void CreateFolder(string path);
        Task WriteBytesAsync(string filePath, byte[] bytes);
        Task WriteStringAsync(string filePath, string content);
    }

    [ExcludeFromCodeCoverage]
    internal class FileService : IFileService
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void Delete(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void Move(string sourceFileName, string destFileName)
        {
            if (!Exists(sourceFileName))
            {
                throw new InvalidOperationException($"Source file name not found: {sourceFileName}");
            }

            if (Exists(destFileName))
            {
                Delete(destFileName);
            }
            else
            {
                EnsureFolderExistanceForFile(destFileName);
            }

            File.Move(sourceFileName, destFileName);
        }

        public void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public async Task WriteBytesAsync(string filePath, byte[] bytes)
        {
            EnsureFolderExistanceForFile(filePath);

            using (var stream = File.OpenWrite(filePath))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public async Task WriteStringAsync(string filePath, string content)
        {
            EnsureFolderExistanceForFile(filePath);

            using (var stream = File.OpenWrite(filePath))
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(content);
                }
            }
        }

        private void EnsureFolderExistanceForFile(string filePath)
        {
            var folder = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
    }
}
