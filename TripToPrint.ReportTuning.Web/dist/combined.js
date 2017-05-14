var TripToPrint;
(function (TripToPrint) {
    class App {
        constructor() {
            this.init();
        }
        init() {
            let rootElement = React.createElement(TripToPrint.Root, {});
            this.root = ReactDOM.render(rootElement, document.getElementById("root"));
            host.documentInitialized();
        }
        applyData(data) {
            this.root.setState({ document: data });
        }
    }
    TripToPrint.App = App;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Group extends React.Component {
        render() {
            let group = this.props.group;
            return React.createElement("div", null,
                this.renderOverviewMap(),
                group.isRoute ? this.renderRoute() : this.renderPoints());
        }
        renderOverviewMap() {
            let className = "ov";
            if (!this.props.isFirst) {
                className += " ov-notfirst";
            }
            return React.createElement("div", { className: className },
                React.createElement("h4", { className: "title" }, this.props.section.name),
                React.createElement("img", { src: this.props.group.overviewMapFilePath }));
        }
        renderRoute() {
            let group = this.props.group;
            return React.createElement("div", { className: "dir" }, group.placemarks.map(pm => React.createElement(TripToPrint.Placemark, { placemark: pm, isInRouteGroup: group.isRoute })));
        }
        renderPoints() {
            let group = this.props.group;
            let totalImagesCount = group.placemarks.map(x => Math.max(1, x.images.length) - 1)
                .reduce((prev, curr) => prev + curr, 0);
            let meaningSizeOfGroup = group.placemarks.length + totalImagesCount;
            let placemarksInColumn1 = [];
            let placemarksInColumn2 = [];
            var incrementalColumnSize = 0;
            for (let i = 0; i < group.placemarks.length; i++) {
                let pm = group.placemarks[i];
                let pmElement = React.createElement(TripToPrint.Placemark, { placemark: pm, isInRouteGroup: group.isRoute });
                if (incrementalColumnSize >= meaningSizeOfGroup / 2) {
                    placemarksInColumn2.push(pmElement);
                }
                else {
                    placemarksInColumn1.push(pmElement);
                }
                incrementalColumnSize += Math.max(1, pm.images.length);
            }
            return React.createElement("div", { className: "pm-cols" },
                React.createElement("div", { className: "pm-col" }, placemarksInColumn1),
                React.createElement("div", { className: "pm-col" }, placemarksInColumn2));
        }
    }
    TripToPrint.Group = Group;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Placemark extends React.Component {
        render() {
            let pm = this.props.placemark;
            return React.createElement("div", { className: "pm" },
                React.createElement("img", { className: "icon", src: pm.iconPath }),
                this.renderThumbnailMap(),
                React.createElement("div", { className: "header" },
                    React.createElement("span", { className: "coord" },
                        "(",
                        React.createElement("a", { href: `http://maps.google.com/?ll=${pm.coordinates[0]}`, onClick: this.preventNavigation }, pm.coordinates[0]),
                        ")"),
                    React.createElement("span", { className: "title" },
                        " ",
                        pm.name),
                    this.renderDistance()),
                pm.description ? React.createElement("div", { className: "pm-desc", dangerouslySetInnerHTML: { __html: pm.description } }) : null,
                this.renderImages(pm.images),
                this.renderDiscoveredData(pm.discoveredData));
        }
        renderThumbnailMap() {
            let pm = this.props.placemark;
            if (this.props.isInRouteGroup) {
                return null;
            }
            return React.createElement("div", null,
                React.createElement("img", { className: "map", src: pm.thumbnailFilePath }),
                React.createElement("div", { className: "ix" }, pm.index));
        }
        renderDistance() {
            let pm = this.props.placemark;
            if (!pm.isShape) {
                return null;
            }
            return React.createElement("span", { className: "dist" },
                " (",
                pm.distance,
                ")");
        }
        renderImages(images) {
            if (images.length == 0)
                return null;
            return React.createElement("div", { className: "pm-img" }, images.map(x => React.createElement("img", { src: x, onError: this.onImageError })));
        }
        onImageError(event) {
            let element = event.target;
            element.style.display = "none";
            console.log(`Image not loaded: ${element.getAttribute("src")}`);
        }
        renderDiscoveredData(discovered) {
            if (discovered == null)
                return null;
            let output = "";
            let sep = null;
            if (discovered.address) {
                output = discovered.address;
                sep = " | ";
            }
            if (discovered.contactPhone) {
                output += sep + discovered.contactPhone;
                sep = " | ";
            }
            if (discovered.website) {
                output += sep + discovered.website;
            }
            return React.createElement("div", { className: "pm-xtra" },
                React.createElement("hr", null),
                output,
                discovered.openingHours ? React.createElement("span", null,
                    React.createElement("br", null),
                    "Opening hours: ",
                    discovered.openingHours) : null,
                discovered.wikipediaContent ? React.createElement("span", null,
                    React.createElement("br", null),
                    "Wikipedia: ",
                    React.createElement("span", { dangerouslySetInnerHTML: { __html: discovered.wikipediaContent } })) : null);
        }
        preventNavigation(event) {
            event.stopPropagation();
            event.nativeEvent.stopImmediatePropagation();
            return false;
        }
    }
    TripToPrint.Placemark = Placemark;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Root extends React.Component {
        constructor() {
            super();
            this.state = {
                document: null
            };
        }
        render() {
            let doc = this.state.document;
            if (doc == null)
                return null;
            return React.createElement("div", null,
                React.createElement("h3", null, doc.title),
                (doc.description != null ? React.createElement("p", { className: "doc-desc", dangerouslySetInnerHTML: { __html: doc.description } }) : null),
                this.renderSections());
        }
        renderSections() {
            let sections = this.state.document.sections;
            return sections.map((s, i) => React.createElement(TripToPrint.Section, { section: s, isFirst: i == 0 }));
        }
    }
    TripToPrint.Root = Root;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Section extends React.Component {
        render() {
            return React.createElement("div", { className: "folder" }, this.props.section.groups.map((group, i) => React.createElement(TripToPrint.Group, { group: group, section: this.props.section, isFirst: this.props.isFirst && i === 0 })));
        }
    }
    TripToPrint.Section = Section;
})(TripToPrint || (TripToPrint = {}));
//# sourceMappingURL=combined.js.map