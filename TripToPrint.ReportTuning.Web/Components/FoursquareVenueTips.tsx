module TripToPrint {
    export interface IFoursquareVenueTipsProps {
        tips: Interfaces.IFoursquareVenueTipDto[];
    }

    export class FoursquareVenueTips extends Hideable<IFoursquareVenueTipsProps> {
        renderUnhidden() {
            return <div className="pm-xtra-tips">
                       <hr />
                       {this.props.tips.map(tip => this.renderTip(tip))}
                       <Commands>
                           <CommandHide onClick={() => { this.hide(); }}/>
                       </Commands>
                   </div>;
        }

        private renderTip(tip: Interfaces.IFoursquareVenueTipDto) {
            let disaggrees = tip.disagreeCount > 0 ? ` ${tip.disagreeCount} 👎` : "";
            let likes = tip.likes > 0 ? ` ${tip.likes} ❤` : "";
            return <p>— {tip.message} ({tip.agreeCount}👍{disaggrees}{likes})</p>;
        }
    }
}
