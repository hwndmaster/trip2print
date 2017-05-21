module TripToPrint {
    export interface IOverviewMapProps {
        group: Interfaces.IMooiGroupDto;
        section: Interfaces.IMooiSectionDto;
        isFirst: boolean;
    }

    export class OverviewMap extends Hideable<IOverviewMapProps> {
        renderUnhidden() {
            let className = "ov";

            if (!this.props.isFirst) {
                className += " ov-notfirst";
            }

            return <div className={className}>
                       <h4 className="title">{this.props.section.name}</h4>
                       <img src={this.props.group.overviewMapFilePath}/>
                       <Commands>
                           <CommandHide onClick={() => { this.hide(); }}/>
                       </Commands>
                   </div>;
        }
    }
}
