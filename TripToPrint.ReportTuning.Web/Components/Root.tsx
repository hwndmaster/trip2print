module TripToPrint {
    export interface IRootState {
        document: Interfaces.IMooiDocumentDto
    }

    export class Root extends React.Component<{}, IRootState> {
        constructor() {
            super();

            this.state = {
                document: null
            };
        }

        render() {
            const doc = this.state.document;

            if (doc == null)
                return null;

            return <div>
                       <h3>{doc.title}</h3>
                       {(doc.description != null
                           ? <p className="doc-desc" dangerouslySetInnerHTML={{ __html: doc.description }} />
                           : null)}
                       {this.renderSections()}
                   </div>;
        }

        private renderSections() {
            const sections = this.state.document.sections;

            return sections.map((s, i) => <Section section={s} isFirst={i === 0}/>);
        }
    }
}
