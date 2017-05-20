module TripToPrint
{
    export class Commands extends React.Component<{}, {}>
    {
        render() {
            const className = "commands-ctr";

            return <div className={className} ref="ctr">
                       <div className="commands-inner">
                           {this.props.children}
                       </div>
                   </div>;
        }

        componentDidMount() {
            const me = this.refs["ctr"] as HTMLElement;
            me.parentElement.classList.add("commands-parent");
        }
    }
}
