module TripToPrint
{
    export interface ISectionProps {
        section: Interfaces.IMooiSectionDto;
        isFirst: boolean;
    }

    export class Section extends React.Component<ISectionProps, {}>
    {
        render() {
            return <div className="folder">
                {this.props.section.clusters.map((cluster, i) =>
                    <Cluster cluster={cluster} section={this.props.section} isFirst={this.props.isFirst && i === 0} />
                )}
            </div>;
        }
    }
}
