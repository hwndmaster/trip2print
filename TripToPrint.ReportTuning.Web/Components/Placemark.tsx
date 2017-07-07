module TripToPrint {
    export interface IPlacemarkProps {
        placemark: Interfaces.IMooiPlacemarkDto,
        isInRouteGroup: boolean;
    }

    export class Placemark extends React.Component<IPlacemarkProps, {}> {
        private static readonly SOURCE_TYPE_HERE = "Here";
        private static readonly SOURCE_TYPE_FOURSQUARE = "Foursquare";

        render() {
            let pm = this.props.placemark;

            return <div className="pm">
                       {this.props.isInRouteGroup ? null : <ThumbnailMap placemark={pm} />}
                       {this.props.isInRouteGroup ? <img className="small-icon" src={pm.iconPath} /> : null}
                       <div className="header">
                           <span className="coord">
                               (<a href={`http://maps.google.com/?ll=${pm.coordinates[0]}`} onClick={this.preventNavigation}>{pm.coordinates[0]}</a>)
                           </span>
                           <span className="title"> {pm.name}</span>
                           {this.renderDistance()}
                       </div>
                       {pm.description
                           ? <div className="pm-desc" dangerouslySetInnerHTML={{ __html: pm.description }} />
                           : null}
                       {this.renderImages(pm.images)}
                       {pm.attachedVenues == null ? null : pm.attachedVenues.map(av => this.renderVenue(av))}
                   </div>;
        }

        private renderDistance() {
            let pm = this.props.placemark;

            if (!pm.isShape) {
                return null;
            }

            return <span className="dist"> ({pm.distance})</span>;
        }

        private renderImages(images: string[]) {
            if (images.length === 0)
                return null;

            return images.map(x => <PlacemarkImage imageUrl={x}/>);
        }

        private renderVenue(venue: Interfaces.IVenueBaseDto) {
            if (venue == null)
                return null;

            if (venue.sourceType === Placemark.SOURCE_TYPE_HERE) {
                return <HereVenue venue={venue as Interfaces.IHereVenueDto} />;
            }
            else if (venue.sourceType === Placemark.SOURCE_TYPE_FOURSQUARE) {
                return <FoursquareVenue venue={venue as Interfaces.IFoursquareVenueDto}/>;
            }

            throw new Error(`This type of venue is not supported: ${venue.sourceType}`);
        }

        private preventNavigation(event: React.MouseEvent<HTMLAnchorElement>) {
            event.stopPropagation();
            event.nativeEvent.stopImmediatePropagation();
            return false;
        }
    }
}
