using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Ionic.Zip;

namespace TripToPrint.Core
{
    public interface IZipFileWrapper : IDisposable
    {
        IEnumerable<string> GetFileNames();
        byte[] GetFileBytes(string fileName);
        Task<string> GetFileContent(string fileName);
        void SaveToFolder(string filename, string path);
    }

    [ExcludeFromCodeCoverage]
    internal class ZipFileWrapper : IZipFileWrapper
    {
        private readonly ZipFile _zip;

        public ZipFileWrapper(string zipFileName)
        {
            _zip = ZipFile.Read(zipFileName);
        }

        public IEnumerable<string> GetFileNames()
        {
            return _zip.Entries.Select(x => x.FileName);
        }

        public byte[] GetFileBytes(string fileName)
        {
            var entry = GetEntryOrThrow(fileName);

            using (var stream = new MemoryStream(new byte[entry.UncompressedSize]))
            {
                entry.Extract(stream);

                stream.Seek(0, SeekOrigin.Begin);

                return stream.ToArray();
            }
        }

        public async Task<string> GetFileContent(string fileName)
        {
            var entry = GetEntryOrThrow(fileName);

            using (var stream = new MemoryStream(new byte[entry.UncompressedSize]))
            {
                entry.Extract(stream);

                stream.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public void SaveToFolder(string filename, string path)
        {
            var entry = GetEntryOrThrow(filename);
            entry.Extract(path, ExtractExistingFileAction.OverwriteSilently);
        }

        public void Dispose()
        {
            _zip.Dispose();
        }

        private ZipEntry GetEntryOrThrow(string fileName)
        {
            var entry = _zip.Entries.FirstOrDefault(x => x.FileName == fileName);
            if (entry == null)
            {
                throw new InvalidOperationException($"File with name '{fileName}' was not found in the zip file");
            }

            return entry;
        }
    }
}
