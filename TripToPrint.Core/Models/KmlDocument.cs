using System.Collections.Generic;
using System.Linq;

namespace TripToPrint.Core.Models
{
    public class KmlDocument
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<KmlFolder> Folders { get; } = new List<KmlFolder>();
        public List<KmlResource> Resources { get; } = new List<KmlResource>();

        public KmlDocument CloneWithExcluding(IKmlElement[] elementsToExclude)
        {
            var cloned = new KmlDocument {
                Title = this.Title,
                Description = this.Description
            };

            cloned.Resources.AddRange(this.Resources); // Not necessary to do a deep clone
            cloned.Folders.AddRange(this.Folders
                .Where(f => !elementsToExclude.Contains(f))
                .Select(f => f.CloneWithExcluding(elementsToExclude)));

            return cloned;
        }
    }
}
