module TripToPrint {
    export interface IClusterProps {
        cluster: Interfaces.IMooiClusterDto;
        section: Interfaces.IMooiSectionDto;
        isFirst: boolean;
    }

    export class Cluster extends React.Component<IClusterProps, {}> {
        render() {
            const cluster = this.props.cluster;

            return <div>
                       <OverviewMap cluster={cluster} section={this.props.section} isFirst={this.props.isFirst}/>
                       {cluster.isRoute ? this.renderRoute() : this.renderPoints()}
                   </div>;
        }

        private renderRoute() {
            let cluster = this.props.cluster;

            return <div>
                       {cluster.placemarks.map(pm =>
                           <Placemark placemark={pm} isInRouteCluster={cluster.isRoute}/>
                       )}
                   </div>;
        }

        private renderPoints() {
            let cluster = this.props.cluster;

            let totalImagesCount = cluster.placemarks.map(x => Math.max(1, x.images.length) - 1)
                .reduce((prev, curr) => prev + curr, 0);
            let meaningSizeOfCluster = cluster.placemarks.length + totalImagesCount;

            let placemarksInColumn1: JSX.Element[] = [];
            let placemarksInColumn2: JSX.Element[] = [];
            var incrementalColumnSize = 0;
            for (let i = 0; i < cluster.placemarks.length; i++) {
                let pm = cluster.placemarks[i];
                let pmElement = <Placemark placemark={pm} isInRouteCluster={cluster.isRoute}/>;
                if (incrementalColumnSize >= meaningSizeOfCluster / 2) {
                    placemarksInColumn2.push(pmElement);
                } else {
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
