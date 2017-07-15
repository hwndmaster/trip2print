module TripToPrint {
    export abstract class VenueBase<TP> extends Hideable<TP> {
        constructor(props) {
            super(props);
        }

        protected createVenueBaseString(venue: Interfaces.IVenueBaseDto) {
            let output = "";
            let sep = "";

            if (venue.address) {
                output = venue.address;
                sep = " | ";
            }
            if (venue.contactPhone) {
                output += sep + venue.contactPhone;
                sep = " | ";
            }
            if (venue.website) {
                output += sep + venue.website;
            }

            return output;
        }

        protected renderOpeningHours(venue: Interfaces.IVenueBaseDto) {
            return venue.openingHours
                ? <span><br />Opening hours: {venue.openingHours}</span>
                : null;
        }
    }
}
