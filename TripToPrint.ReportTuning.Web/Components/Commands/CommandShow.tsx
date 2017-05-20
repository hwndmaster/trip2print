module TripToPrint
{
    export interface ICommandShowProps {
        onClick: React.MouseEventHandler<HTMLButtonElement>;
    }

    export class CommandShow extends React.Component<ICommandShowProps, {}>
    {
        render() {
            return <button onClick={this.props.onClick} title="Show hidden content">
                       <img src="Images/Play.png"/>
                   </button>;
        }
    }
}
