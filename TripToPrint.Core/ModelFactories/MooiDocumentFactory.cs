using System.Collections.Generic;

using TripToPrint.Core.Models;

namespace TripToPrint.Core.ModelFactories
{
    public interface IMooiDocumentFactory
    {
        MooiDocument Create(KmlDocument kmlDocument);
    }

    public class MooiDocumentFactory : IMooiDocumentFactory
    {
        private readonly IMooiGroupFactory _mooiGroupFactory;

        public MooiDocumentFactory(IMooiGroupFactory mooiGroupFactory)
        {
            _mooiGroupFactory = mooiGroupFactory;
        }

        public MooiDocument Create(KmlDocument kmlDocument)
        {
            var model = new MooiDocument {
                Title = kmlDocument.Title,
                Description = kmlDocument.Description,
                Sections = new List<MooiSection>()
            };

            foreach (var folder in kmlDocument.Folders)
            {
                var section = new MooiSection {
                    Document = model,
                    Name = folder.Name,
                    Groups = new List<MooiGroup>()
                };
                model.Sections.Add(section);

                ExtractGroupsFromFolderIntoSection(folder, section);
            }

            return model;
        }

        private void ExtractGroupsFromFolderIntoSection(KmlFolder folder, MooiSection section)
        {
            var groups = _mooiGroupFactory.CreateList(folder);
            groups.ForEach(x => x.Section = section);
            section.Groups.AddRange(groups);
        }
    }
}
