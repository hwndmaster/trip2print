namespace TripToPrint.Core.Models.Integration
{
    internal class FoursquareResponse<TResponse>
    {
        public FoursquareResponseMeta Meta;
        public TResponse Response;
    }

    internal class FoursquareSearchResponseBody
    {
        public FoursquareResponseVenue[] Venues;
    }

    internal class FoursquareExploreResponseBody
    {
        public FoursquareResponseAttributeGroup<FoursquareResponseExploreItem>[] Groups;
    }

    internal class FoursquareDetailsResponseBody
    {
        public FoursquareResponseVenue Venue;
    }

    internal class FoursquareResponseMeta
    {
        public int Code;
        public string ErrorType;
        public string ErrorDetail;
    }

    internal class FoursquareResponseExploreItem
    {
        public FoursquareResponseVenue Venue;
        public FoursquareResponseVenueTip[] Tips;
    }

    internal class FoursquareResponseVenue
    {
        public string Id;
        public string Name;
        public int CreatedAt;
        public string CanonicalUrl;
        public FoursquareResponseVenueCategory[] Categories;
        public FoursquareResponseVenueLocation Location;
        public FoursquareResponseVenueContact Contact;
        public FoursquareResponseVenuePhoto BestPhoto;
        public FoursquareResponseAttributeCollection<FoursquareResponseVenuePhoto> Photos;
        public FoursquareResponseVenuePrice Price;
        public double Rating;
        public string RatingColor;
        public string Url;
        public string ShortUrl;
        public FoursquareResponseVenueStats Stats;
        public FoursquareResponseAttributeCollection<FoursquareResponseVenueTip> Tips;
        public FoursquareResponseAttributeCollection<FoursquareResponseDummy> Likes;
        public FoursquareResponseVenueHours Hours;
        public string[] Tags;
    }

    internal class FoursquareResponseVenueCategory
    {
        public FoursquareResponseResourceUrl Icon;
        public string Id;
        public string Name;
        public string PluralName;
        public string ShortName;
        public bool Primary;
    }

    internal class FoursquareResponseVenueContact
    {
        public string FormattedPhone;
        public string Phone;
        public string Twitter;
        public string Facebook;
        public string FacebookName;
        public string FacebookUsername;
    }

    internal class FoursquareResponseVenueLocation
    {
        public string Address;
        public string Cc;
        public string City;
        public string Country;
        public string[] FormattedAddress;
        public string PostalCode;
        public string State;
        public double Lat;
        public double Lng;
    }

    internal class FoursquareResponseResourceUrl
    {
        public string Prefix;
        public string Suffix;
    }

    internal class FoursquareResponseAttributeCollection<T>
    {
        public int Count;
        public FoursquareResponseAttributeGroup<T>[] Groups;
    }

    internal class FoursquareResponseAttributeGroup<T>
    {
        public int Count;
        public T[] Items;
        public string Name;
        public string Type;
    }

    internal class FoursquareResponseVenuePhoto : FoursquareResponseResourceUrl
    {
        public string Id;
        public string Visibility;
        public int Height;
        public int Width;
        public long CreatedAt;
    }

    internal class FoursquareResponseVenuePrice
    {
        public string Currency;
        public string Message;
        public int Tier;
    }

    internal class FoursquareResponseVenueStats
    {
        public int CheckinsCount;
        public int TipCount;
        public int UsersCount;
        public int VisitsCount;
    }

    internal class FoursquareResponseVenueTip
    {
        public int AgreeCount;
        public int DisagreeCount;
        public FoursquareResponseAttributeCollection<FoursquareResponseUser> Likes;
        public int CreatedAt;
        public string CanonicalUrl;
        public string Lang;
        public string Text;
        public string Type;
        public FoursquareResponseUser User;
    }

    internal class FoursquareResponseUser
    {
        public string FirstName;
        public string LastName;
        public string Gender;
        public string Id;
        public FoursquareResponseResourceUrl Photo;
    }

    internal class FoursquareResponseVenueHours
    {
        public FoursquareResponseVenueHoursTimeframe[] Timeframes;
    }

    internal class FoursquareResponseVenueHoursTimeframe
    {
        public string Days;
        public FoursquareResponseVenueHoursTimeframeOpen[] Open;
    }

    public class FoursquareResponseVenueHoursTimeframeOpen
    {
        public string RenderedTime;
    }

    internal class FoursquareResponseDummy
    {
    }
}
