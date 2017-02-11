using System.Collections.Generic;

namespace TripToPrint.Core.Models
{
    public class MooiSection
    {
        public MooiSection()
        {
            Groups = new List<MooiGroup>();
        }

        public MooiDocument Document { get; set; }
        public string Name { get; set; }
        public List<MooiGroup> Groups { get; set; }
    }
}
