/// <reference path="Hideable.tsx"/>
/// <reference path="VenueBase.tsx"/>

module TripToPrint {
    export interface IFoursquareVenueProps {
        venue: Interfaces.IFoursquareVenueDto
    }

    export class FoursquareVenue extends VenueBase<IFoursquareVenueProps> {
        constructor(props) {
            super(props);
        }

        renderUnhidden() {
            let venue = this.props.venue;

            return <div className="pm-xtra">
                       <hr/>
                       {this.renderRating(venue)}
                       {venue.likesCount != null ? <div className="v-prop">{venue.likesCount} ❤</div> : null}
                       <div className="v-prop">{venue.category}</div>
                       {this.renderPrice(venue)}
                       <div>{this.createVenueBaseString(venue)}</div>
                       {this.renderOpeningHours(venue)}
                       <Commands>
                           <CommandHide onClick={() => { this.hide(); }}/>
                       </Commands>
                   </div>;
        }

        private renderRating(venue: Interfaces.IFoursquareVenueDto) {
            if (venue.rating == null || venue.rating === 0) {
                return null;
            }

            const style = {
                background: "#" + venue.ratingColor
            };

            return <div className="v-rating" style={style}>
                {venue.rating}
                <span className="v-maxrating">/{venue.maxRating}</span>
            </div>;
        }

        private renderPrice(venue: Interfaces.IFoursquareVenueDto) {
            if (venue.priceLevel == null) {
                return null;
            }

            return <div className="v-prop v-price">
                <span className="v-pricelvl">{venue.priceLevel}</span>
                <span className="v-rempricelvl">{venue.remainingPriceLevel}</span>
            </div>;
        }
    }
}
