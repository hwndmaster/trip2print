module TripToPrint {
    export interface IThumbnailMapProps {
        placemark: Interfaces.IMooiPlacemarkDto
    }

    export class ThumbnailMap extends Hideable<IThumbnailMapProps> {
        constructor(props) {
            super(props);
        }

        renderUnhidden() {
            let pm = this.props.placemark;

            return <div className="thumbnail-map-ctr">
                <img className="map" src={pm.thumbnailFilePath} />
                <img className="icon" src={pm.iconPath} />
                <div className="ix">{pm.index}</div>
                <Commands>
                    <CommandHide onClick={() => { this.hide(); }} />
                </Commands>
            </div>;
        }
    }
}
