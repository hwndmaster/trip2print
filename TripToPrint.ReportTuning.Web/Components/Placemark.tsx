module TripToPrint
{
    export interface IPlacemarkProps {
        placemark: Interfaces.IMooiPlacemarkDto,
        isInRouteGroup: boolean;
    }

    export class Placemark extends React.Component<IPlacemarkProps, {}>
    {
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
                {pm.description ? <div className="pm-desc" dangerouslySetInnerHTML={{ __html: pm.description }} /> : null}
                {this.renderImages(pm.images)}
                {this.renderDiscoveredData(pm.discoveredData)}
            </div>;
        }

        private renderDistance() {
            let pm = this.props.placemark;

            if (!pm.isShape) {
                return null;
            }

            return <span className="dist"> ({pm.distance})</span>
        }

        private renderImages(images: string[]) {
            if (images.length == 0)
                return null;

            return <div className="pm-img">
                {images.map(x => <img src={x} onError={this.onImageError} />)}
            </div>;
        }

        private onImageError(event: React.SyntheticEvent<HTMLImageElement>) {
            let element = event.target as HTMLElement;
            element.style.display = "none";
            console.log(`Image not loaded: ${element.getAttribute("src")}`);
        }

        private renderDiscoveredData(discovered: Interfaces.IDiscoveredPlaceDto) {
            if (discovered == null)
                return null;

            let output = "";
            let sep = null;

            if (discovered.address) {
                output = discovered.address;
                sep = " | ";
            }
            if (discovered.contactPhone) {
                output += sep + discovered.contactPhone;
                sep = " | ";
            }
            if (discovered.website) {
                output += sep + discovered.website;
            }

            return <div className="pm-xtra">
                <hr />
                {output}
                {discovered.openingHours ? <span><br />Opening hours: {discovered.openingHours}</span> : null}
                {discovered.wikipediaContent ? <span><br />Wikipedia: <span dangerouslySetInnerHTML={{ __html: discovered.wikipediaContent }} /></span> : null}
            </div>;
        }

        private preventNavigation(event: React.MouseEvent<HTMLAnchorElement>) {
            event.stopPropagation();
            event.nativeEvent.stopImmediatePropagation();
            return false;
        }
    }
}
