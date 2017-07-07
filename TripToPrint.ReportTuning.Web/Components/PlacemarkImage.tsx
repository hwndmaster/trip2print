module TripToPrint {
    export interface IPlacemarkImageProps {
        imageUrl: string;
    }

    export class PlacemarkImage extends Hideable<IPlacemarkImageProps> {
        renderUnhidden() {
            return <div className="pm-img-item">
                       <img src={this.props.imageUrl} onError={this.onImageError} />
                       <Commands>
                           <CommandHide onClick={() => { this.hide(); }}/>
                           <CommandZoomIn onClick={() => { this.zoom(true); }} />
                           <CommandZoomOut onClick={() => { this.zoom(false); }} />
                       </Commands>
                   </div>;
        }

        private zoom(zoomIn: boolean) {
            
        }

        private onImageError(event: React.SyntheticEvent<HTMLImageElement>) {
            let element = event.target as HTMLElement;
            element.style.display = "none";
            console.log(`Image not loaded: ${element.getAttribute("src")}`);
        }
    }
}
