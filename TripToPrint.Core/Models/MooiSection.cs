using System.Collections.Generic;

namespace TripToPrint.Core.Models
{
    public class MooiSection
    {
        public MooiDocument Document { get; set; }
        public string Name { get; set; }
        public List<MooiGroup> Groups { get; } = new List<MooiGroup>();
    }
}
