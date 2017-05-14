module TripToPrint
{
    export interface IGroupProps {
        group: Interfaces.IMooiGroupDto
        section: Interfaces.IMooiSectionDto,
        isFirst: boolean;
    }

    export class Group extends React.Component<IGroupProps, {}>
    {
        render() {
            let group = this.props.group;

            return <div>
                {this.renderOverviewMap()}
                {group.isRoute ? this.renderRoute() : this.renderPoints()}
            </div>;
        }

        private renderOverviewMap() {
            let className = "ov";

            if (!this.props.isFirst) {
                className += " ov-notfirst";
            }

            return <div className={className}>
                <h4 className="title">{this.props.section.name}</h4>
                <img src={this.props.group.overviewMapFilePath} />
            </div>
        }

        private renderRoute() {
            let group = this.props.group;

            return <div className="dir">
                {group.placemarks.map(pm =>
                    <Placemark placemark={pm} isInRouteGroup={group.isRoute} />
                )}
            </div>;
        }

        private renderPoints() {
            let group = this.props.group;

            let totalImagesCount = group.placemarks.map(x => Math.max(1, x.images.length) - 1)
                .reduce((prev, curr) => prev + curr, 0);
            let meaningSizeOfGroup = group.placemarks.length + totalImagesCount;

            let placemarksInColumn1: JSX.Element[] = [];
            let placemarksInColumn2: JSX.Element[] = [];
            var incrementalColumnSize = 0;
            for (let i = 0; i < group.placemarks.length; i++) {
                let pm = group.placemarks[i];
                let pmElement = <Placemark placemark={pm} isInRouteGroup={group.isRoute} />;
                if (incrementalColumnSize >= meaningSizeOfGroup / 2) {
                    placemarksInColumn2.push(pmElement);
                }
                else {
                    placemarksInColumn1.push(pmElement);
                }

                incrementalColumnSize += Math.max(1, pm.images.length);
            }

            return <div className="pm-cols">
                <div className="pm-col">
                    {placemarksInColumn1}
                </div>
                <div className="pm-col">
                    {placemarksInColumn2}
                </div>
            </div>;
        }
    }
}
