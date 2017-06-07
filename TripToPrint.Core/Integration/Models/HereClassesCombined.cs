namespace TripToPrint.Core.Integration.Models
{
    internal class HereDiscoverSearchResponse
    {
        public HereDiscoverSearchResult Results;
    }

    internal class HereDiscoverSearchResult
    {
        public HereDiscoverSearchResultItem[] Items;
    }

    internal class HereDiscoverSearchResultItem
    {
        public string Id;
        public string Title;
        public double Distance;
        public double[] Position;
        public HerePlaceCategory Category;
        public string Icon;
        public string Vicinity;
        public string Href;
        public HerePlaceOpeningHours OpeningHours;
    }

    internal class HerePlaceCategory
    {
        public string Title;
    }

    internal class HerePlaceOpeningHours
    {
        public string Text;
    }

    internal class HerePlace
    {
        public string PlaceId;
        public string Name;
        public string View;
        public HerePlaceLocation Location;
        public HerePlaceContacts Contacts;
        public HerePlaceTag[] Tags;
        public HerePlaceMedia Media;
    }

    internal class HerePlaceLocation
    {
        public HerePlaceLocationAddress Address;
    }

    internal class HerePlaceLocationAddress
    {
        public string Street;
        public string City;
        public string State;
        public string Country;
        public string CountryCode;
        public string Text;
    }

    internal class HerePlaceContacts
    {
        public HerePlaceContactItem[] Phone;
        public HerePlaceContactItem[] Website;
    }

    internal class HerePlaceContactItem
    {
        public string Label;
        public string Value;
    }

    internal class HerePlaceTag
    {
        public string Id;
        public string Group;
        public string Title;
    }

    internal class HerePlaceMedia
    {
        public HerePlaceEditorials Editorials;
    }

    internal class HerePlaceEditorials
    {
        public HerePlaceEditorialsItem[] Items;
    }

    internal class HerePlaceEditorialsItem
    {
        public string Attribution;
        public string Description;
        public string Language;
        public HerePlaceEditorialsSupplier Supplier;
        public HerePlaceEditorialsVia Via;
    }

    internal class HerePlaceEditorialsSupplier
    {
        public string Href;
        public string Icon;
        public string Id;
        public string Title;
        public string Type;
    }

    internal class HerePlaceEditorialsVia
    {
        public string Href;
        public string Type;
    }
}
