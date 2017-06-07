using System.Collections.Generic;
using System.Xml.Linq;

namespace TripToPrint.Core.ExtensionMethods
{
    internal static class Xml
    {
        public static XElement ElementByLocalName(this XElement xelement, string name)
        {
            return xelement.Element(xelement.ResolveName(name));
        }

        public static IEnumerable<XElement> ElementsByLocalName(this XElement xelement, string name)
        {
            return xelement.Elements(xelement.ResolveName(name));
        }

        public static XName ResolveName(this XObject xObj, XName name)
        {
            // If no namespace has been added, use default namespace anyway
            if (string.IsNullOrEmpty(name.NamespaceName))
            {
                name = xObj.Document?.Root?.GetDefaultNamespace() + name.LocalName;
            }
            return name;
        }
    }
}
