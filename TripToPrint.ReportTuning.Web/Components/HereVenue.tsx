/// <reference path="Hideable.tsx"/>
/// <reference path="VenueBase.tsx"/>

module TripToPrint {
    export interface IHereVenueProps {
        venue: Interfaces.IHereVenueDto
    }

    export class HereVenue extends VenueBase<IHereVenueProps> {
        constructor(props) {
            super(props);
        }

        renderUnhidden() {
            const venue = this.props.venue;

            return <div className="pm-xtra">
                       <hr />
                       {this.createVenueBaseString(venue)}
                       {this.renderOpeningHours(venue)}
                       {venue.wikipediaContent
                           ? <span><br />Wikipedia: <span dangerouslySetInnerHTML={{ __html: venue.wikipediaContent }} /></span>
                           : null}
                   </div>;
        }
    }
}
