module TripToPrint {
    export class CommandHide extends BaseCommand {
        getTitle() { return "Hide this in report"; }

        getImageName() { return "Power.png"; }
    }
}
