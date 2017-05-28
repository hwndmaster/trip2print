module TripToPrint {
    export interface IThumbnailMapProps {
        placemark: Interfaces.IMooiPlacemarkDto
    }

    export class ThumbnailMap extends Hideable<IThumbnailMapProps> {
        private static FOURSQUARE_ICON_DOMAIN = /4sqi\.net/;

        constructor(props) {
            super(props);
        }

        renderUnhidden() {
            const pm = this.props.placemark;
            const style = {};

            if (pm.iconPath.match(ThumbnailMap.FOURSQUARE_ICON_DOMAIN)) {
                style["background"] = "lightslategray";
            }

            return <div className="thumbnail-map-ctr">
                <img className="map" src={pm.thumbnailFilePath} />
                <img className="icon" src={pm.iconPath} style={style} />
                <div className="ix">{pm.index}</div>
                <Commands>
                    <CommandHide onClick={() => { this.hide(); }} />
                </Commands>
            </div>;
        }
    }
}
