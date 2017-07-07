module TripToPrint {
    export class CommandZoomIn extends BaseCommand {
        getTitle() { return "Increase size"; }

        getImageName() { return "ZoomIn.png"; }
    }
}
