module TripToPrint {
﻿    export interface IBaseCommandProps {
﻿        onClick: React.MouseEventHandler<HTMLButtonElement>;
﻿    }

     export abstract class BaseCommand extends React.Component<IBaseCommandProps> {
﻿        abstract getTitle(): string;

﻿        abstract getImageName(): string;

﻿        render() {
﻿            const imagePath = `Images/${this.getImageName()}`;
﻿            return <button onClick={this.props.onClick} title={this.getTitle()}>
                        <img src={imagePath}/>
                    </button>;
﻿        }
﻿    }
﻿}
