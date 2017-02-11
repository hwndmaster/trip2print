using System.Collections.Generic;

namespace TripToPrint.Core.Models
{
    public class KmlDocument
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<KmlFolder> Folders { get; set; }
    }
}
