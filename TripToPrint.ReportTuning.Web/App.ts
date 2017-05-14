module TripToPrint
{
    declare var host: Interfaces.IHostGate;

    export class App {
        root: Root;

        constructor() {
            this.init();
        }

        init() {
            let rootElement = React.createElement(Root, {});
            this.root = <Root>ReactDOM.render(rootElement, document.getElementById("root"));

            host.documentInitialized();
        }

        applyData(data: Interfaces.IMooiDocumentDto) {
            this.root.setState({ document: data });
        }
    }
}
