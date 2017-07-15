module TripToPrint {
    export interface IPlacemarkImageProps {
        imageUrl: string;
    }

    export interface IPlacemarkImageStats extends IHideableState {
        zoom: 0;
    }

    export class PlacemarkImage extends HideableWithStatus<IPlacemarkImageProps, IPlacemarkImageStats> {
        private static DEFAULT_MAX_IMAGE_WIDTH = 170;
        private static DEFAULT_MAX_IMAGE_HEIGHT = 120;
        private static ZOOM_STEP = 0.2;

        constructor(props) {
            super(props);
            this.state = { hidden: false, zoom: 1 };
        }

        renderUnhidden() {
            let style = {
                maxWidth: PlacemarkImage.DEFAULT_MAX_IMAGE_WIDTH * this.state.zoom,
                maxHeight: PlacemarkImage.DEFAULT_MAX_IMAGE_HEIGHT * this.state.zoom
            };

            return <div className="pm-img-item">
                       <img src={this.props.imageUrl} onError={this.onImageError} style={style}/>
                       <Commands>
                           <CommandHide onClick={() => { this.hide(); }}/>
                           <CommandZoomIn onClick={() => { this.zoom(true); }}/>
                           <CommandZoomOut onClick={() => { this.zoom(false); }}/>
                       </Commands>
                   </div>;
        }

        private zoom(zoomIn: boolean) {
            let coef = 1 + PlacemarkImage.ZOOM_STEP * (zoomIn ? 1 : -1);
            let newZoom = coef * this.state.zoom;
            this.setState(Utils.extend(this.state, { zoom: newZoom }));
        }

        private onImageError(event: React.SyntheticEvent<HTMLImageElement>) {
            let element = event.target as HTMLElement;
            element.style.display = "none";
            console.log(`Image not loaded: ${element.getAttribute("src")}`);
        }
    }
}
