module TripToPrint
{
    export interface ICommandHideProps {
        onClick: React.MouseEventHandler<HTMLButtonElement>;
    }

    export class CommandHide extends React.Component<ICommandHideProps, {}>
    {
        render() {
            return <button onClick={this.props.onClick} title="Hide this in report">
                       <img src="Images/Power.png"/>
                   </button>;
        }
    }
}
