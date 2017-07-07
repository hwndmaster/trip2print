module TripToPrint {
    export class CommandZoomOut extends BaseCommand {
        getTitle() { return "Decrease size"; }

        getImageName() { return "ZoomOut.png"; }
    }
}
