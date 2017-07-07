module TripToPrint
{
    export interface IHideableState {
        hidden: boolean;
    }

    //export abstract class HideableWithStatus<TP, TS extends IHideableState> extends React.Component<TP, TS> {
    export abstract class Hideable<TP> extends React.Component<TP, IHideableState> {
        constructor(props) {
            super(props);
            this.state = { hidden: false };
        }

        render() {
            if (this.state.hidden) {
                return <div className="hidden">
                           <Commands>
                               <CommandShow onClick={() => { this.show(); }} />
                           </Commands>
                       </div>;
            }

            return this.renderUnhidden();
        }

        abstract renderUnhidden(): JSX.Element;

        protected hide() {
            this.setState({
                hidden: true
            });
        }

        protected show() {
            this.setState({
                hidden: false
            });
        }
    }

    /*export abstract class Hideable<TP> extends HideableWithStatus<TP, IHideableState>
    {
    }*/
}
