using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TripToPrint.Core.ModelFactories;
using TripToPrint.Core.Models;

namespace TripToPrint.Core
{
    public interface IKmlFileReader
    {
        Task<KmlDocument> ReadFromFile(string inputFilePath);
    }

    public class KmlFileReader : IKmlFileReader
    {
        private readonly IKmlDocumentFactory _kmlDocumentFactory;
        private readonly IZipService _zipService;

        public KmlFileReader(IKmlDocumentFactory kmlDocumentFactory, IZipService zipService)
        {
            _kmlDocumentFactory = kmlDocumentFactory;
            _zipService = zipService;
        }

        public async Task<KmlDocument> ReadFromFile(string inputFilePath)
        {
            var ext = Path.GetExtension(inputFilePath);

            if (ext?.Equals(".kmz", StringComparison.OrdinalIgnoreCase) == true)
            {
                return await ReadFromKmzFile(inputFilePath);
            }
            if (ext?.Equals(".kml", StringComparison.OrdinalIgnoreCase) == true)
            {
                return ReadFromKmlFile(inputFilePath);
            }

            throw new NotSupportedException();
        }

        public virtual async Task<KmlDocument> ReadFromKmzFile(string inputFilePath)
        {
            using (var zip = _zipService.Open(inputFilePath))
            {
                var kmlFileName = zip.GetFileNames().FirstOrDefault(x => Path.GetExtension(x)?.Equals(".kml") == true);
                if (kmlFileName == null)
                {
                    throw new InvalidOperationException("Provided KMZ file is invalid. An entry for KML was not found");
                }

                var kmlContent = await zip.GetFileContent(kmlFileName);
                var kmlDocument = _kmlDocumentFactory.Create(kmlContent);

                var resourceEntries = zip.GetFileNames().Where(x => x.StartsWith("images/")).ToList();
                foreach (var filename in resourceEntries)
                {
                    var blob = zip.GetFileBytes(filename);
                    kmlDocument.Resources.Add(new KmlResource { FileName = filename, Blob = blob });
                }

                return kmlDocument;
            }
        }

        public virtual KmlDocument ReadFromKmlFile(string inputFilePath)
        {
            var kmlContent = File.ReadAllText(inputFilePath);

            return _kmlDocumentFactory.Create(kmlContent);
        }
    }
}
