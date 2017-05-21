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
                {this.props.section.groups.map((group, i) =>
                    <Group group={group} section={this.props.section} isFirst={this.props.isFirst && i === 0} />
                )}
            </div>;
        }
    }
}
