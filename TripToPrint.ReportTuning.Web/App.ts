module TripToPrint
{
    declare var host: Interfaces.IHostGate;

    export class App {
        root: Root;

        constructor() {
            this.init();
        }

        init() {
            const rootElement = React.createElement(Root, {});
            this.root = ReactDOM.render(rootElement, document.getElementById("root")) as Root;

            host.documentInitialized();
        }

        applyData(data: Interfaces.IMooiDocumentDto) {
            this.root.setState({ document: data });
        }
    }
}
