using System.Collections.Generic;

namespace TripToPrint.Core.Models
{
    public class MooiDocument
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<MooiSection> Sections { get; } = new List<MooiSection>();
    }
}
