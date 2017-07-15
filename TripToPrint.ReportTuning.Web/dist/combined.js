var TripToPrint;
(function (TripToPrint) {
    class App {
        constructor() {
            this.init();
        }
        init() {
            const rootElement = React.createElement(TripToPrint.Root, {});
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
    class BaseCommand extends React.Component {
        render() {
            const imagePath = `Images/${this.getImageName()}`;
            return React.createElement("button", { onClick: this.props.onClick, title: this.getTitle() },
                React.createElement("img", { src: imagePath }));
        }
    }
    TripToPrint.BaseCommand = BaseCommand;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class CommandHide extends TripToPrint.BaseCommand {
        getTitle() { return "Hide this in report"; }
        getImageName() { return "Power.png"; }
    }
    TripToPrint.CommandHide = CommandHide;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Commands extends React.Component {
        render() {
            const className = "commands-ctr";
            return React.createElement("div", { className: className, ref: "ctr" },
                React.createElement("div", { className: "commands-inner" }, this.props.children));
        }
        componentDidMount() {
            const me = this.refs["ctr"];
            me.parentElement.classList.add("commands-parent");
        }
    }
    TripToPrint.Commands = Commands;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class CommandShow extends TripToPrint.BaseCommand {
        getTitle() { return "Show hidden content"; }
        getImageName() { return "Play.png"; }
    }
    TripToPrint.CommandShow = CommandShow;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class CommandZoomIn extends TripToPrint.BaseCommand {
        getTitle() { return "Increase size"; }
        getImageName() { return "ZoomIn.png"; }
    }
    TripToPrint.CommandZoomIn = CommandZoomIn;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class CommandZoomOut extends TripToPrint.BaseCommand {
        getTitle() { return "Decrease size"; }
        getImageName() { return "ZoomOut.png"; }
    }
    TripToPrint.CommandZoomOut = CommandZoomOut;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class HideableWithStatus extends React.Component {
        render() {
            if (this.state.hidden) {
                return React.createElement("div", { className: "hidden" },
                    React.createElement(TripToPrint.Commands, null,
                        React.createElement(TripToPrint.CommandShow, { onClick: () => { this.show(); } })));
            }
            return this.renderUnhidden();
        }
        hide() {
            this.setState(TripToPrint.Utils.extend(this.state, { hidden: true }));
        }
        show() {
            this.setState(TripToPrint.Utils.extend(this.state, { hidden: false }));
        }
    }
    TripToPrint.HideableWithStatus = HideableWithStatus;
    class Hideable extends HideableWithStatus {
        constructor(props) {
            super(props);
            this.state = { hidden: false };
        }
    }
    TripToPrint.Hideable = Hideable;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class VenueBase extends TripToPrint.Hideable {
        constructor(props) {
            super(props);
        }
        createVenueBaseString(venue) {
            let output = "";
            let sep = "";
            if (venue.address) {
                output = venue.address;
                sep = " | ";
            }
            if (venue.contactPhone) {
                output += sep + venue.contactPhone;
                sep = " | ";
            }
            if (venue.website) {
                output += sep + venue.website;
            }
            return output;
        }
        renderOpeningHours(venue) {
            return venue.openingHours
                ? React.createElement("span", null,
                    React.createElement("br", null),
                    "Opening hours: ",
                    venue.openingHours)
                : null;
        }
    }
    TripToPrint.VenueBase = VenueBase;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class FoursquareVenue extends TripToPrint.VenueBase {
        constructor(props) {
            super(props);
        }
        renderUnhidden() {
            let venue = this.props.venue;
            return React.createElement("div", { className: "pm-xtra" },
                React.createElement("hr", null),
                this.renderRating(venue),
                venue.likesCount != null ? React.createElement("div", { className: "v-prop" },
                    venue.likesCount,
                    " \u2764") : null,
                React.createElement("div", { className: "v-prop" }, venue.category),
                this.renderPrice(venue),
                React.createElement("div", null, this.createVenueBaseString(venue)),
                this.renderOpeningHours(venue),
                React.createElement(TripToPrint.Commands, null,
                    React.createElement(TripToPrint.CommandHide, { onClick: () => { this.hide(); } })));
        }
        renderRating(venue) {
            if (venue.rating == null || venue.rating === 0) {
                return null;
            }
            const style = {
                background: "#" + venue.ratingColor
            };
            return React.createElement("div", { className: "v-rating", style: style },
                venue.rating,
                React.createElement("span", { className: "v-maxrating" },
                    "/",
                    venue.maxRating));
        }
        renderPrice(venue) {
            if (venue.priceLevel == null) {
                return null;
            }
            return React.createElement("div", { className: "v-prop v-price" },
                React.createElement("span", { className: "v-pricelvl" }, venue.priceLevel),
                React.createElement("span", { className: "v-rempricelvl" }, venue.remainingPriceLevel));
        }
    }
    TripToPrint.FoursquareVenue = FoursquareVenue;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class FoursquareVenueTips extends TripToPrint.Hideable {
        renderUnhidden() {
            return React.createElement("div", { className: "pm-xtra-tips" },
                React.createElement("hr", null),
                this.props.tips.map(tip => this.renderTip(tip)),
                React.createElement(TripToPrint.Commands, null,
                    React.createElement(TripToPrint.CommandHide, { onClick: () => { this.hide(); } })));
        }
        renderTip(tip) {
            let disaggrees = tip.disagreeCount > 0 ? ` ${tip.disagreeCount} 👎` : "";
            let likes = tip.likes > 0 ? ` ${tip.likes} ❤` : "";
            return React.createElement("p", null,
                "\u2014 ",
                tip.message,
                " (",
                tip.agreeCount,
                "\uD83D\uDC4D",
                disaggrees,
                likes,
                ")");
        }
    }
    TripToPrint.FoursquareVenueTips = FoursquareVenueTips;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class HereVenue extends TripToPrint.VenueBase {
        constructor(props) {
            super(props);
        }
        renderUnhidden() {
            const venue = this.props.venue;
            return React.createElement("div", { className: "pm-xtra" },
                React.createElement("hr", null),
                this.createVenueBaseString(venue),
                this.renderOpeningHours(venue),
                venue.wikipediaContent
                    ? React.createElement("span", null,
                        React.createElement("br", null),
                        "Wikipedia: ",
                        React.createElement("span", { dangerouslySetInnerHTML: { __html: venue.wikipediaContent } }))
                    : null);
        }
    }
    TripToPrint.HereVenue = HereVenue;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class OverviewMap extends TripToPrint.Hideable {
        renderUnhidden() {
            let className = "ov";
            if (!this.props.isFirst) {
                className += " ov-notfirst";
            }
            return React.createElement("div", { className: className },
                React.createElement("h4", { className: "title" }, this.props.section.name),
                React.createElement("img", { src: this.props.cluster.overviewMapFilePath }),
                React.createElement(TripToPrint.Commands, null,
                    React.createElement(TripToPrint.CommandHide, { onClick: () => { this.hide(); } })));
        }
    }
    TripToPrint.OverviewMap = OverviewMap;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Placemark extends React.Component {
        render() {
            let pm = this.props.placemark;
            return React.createElement("div", { className: "pm" },
                this.props.isInRouteCluster ? null : React.createElement(TripToPrint.ThumbnailMap, { placemark: pm }),
                this.props.isInRouteCluster ? React.createElement("img", { className: "small-icon", src: pm.iconPath }) : null,
                React.createElement("div", { className: "header" },
                    React.createElement("span", { className: "coord" },
                        "(",
                        React.createElement("a", { href: `http://maps.google.com/?ll=${pm.coordinates[0]}`, onClick: this.preventNavigation }, pm.coordinates[0]),
                        ")"),
                    React.createElement("span", { className: "title" },
                        " ",
                        pm.name),
                    this.renderDistance()),
                pm.description
                    ? React.createElement("div", { className: "pm-desc", dangerouslySetInnerHTML: { __html: pm.description } })
                    : null,
                this.renderImages(pm.images),
                pm.attachedVenues == null ? null : pm.attachedVenues.map(av => this.renderVenue(av)));
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
            if (images.length === 0)
                return null;
            return images.map(x => React.createElement(TripToPrint.PlacemarkImage, { imageUrl: x }));
        }
        renderVenue(venue) {
            if (venue == null)
                return null;
            if (venue.sourceType === Placemark.SOURCE_TYPE_HERE) {
                return React.createElement(TripToPrint.HereVenue, { venue: venue });
            }
            else if (venue.sourceType === Placemark.SOURCE_TYPE_FOURSQUARE) {
                return [
                    React.createElement(TripToPrint.FoursquareVenueTips, { tips: venue.tips }),
                    React.createElement(TripToPrint.FoursquareVenue, { venue: venue })
                ];
            }
            throw new Error(`This type of venue is not supported: ${venue.sourceType}`);
        }
        preventNavigation(event) {
            event.stopPropagation();
            event.nativeEvent.stopImmediatePropagation();
            return false;
        }
    }
    Placemark.SOURCE_TYPE_HERE = "Here";
    Placemark.SOURCE_TYPE_FOURSQUARE = "Foursquare";
    TripToPrint.Placemark = Placemark;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class PlacemarkImage extends TripToPrint.HideableWithStatus {
        constructor(props) {
            super(props);
            this.state = { hidden: false, zoom: 1 };
        }
        renderUnhidden() {
            let style = {
                maxWidth: PlacemarkImage.DEFAULT_MAX_IMAGE_WIDTH * this.state.zoom,
                maxHeight: PlacemarkImage.DEFAULT_MAX_IMAGE_HEIGHT * this.state.zoom
            };
            return React.createElement("div", { className: "pm-img-item" },
                React.createElement("img", { src: this.props.imageUrl, onError: this.onImageError, style: style }),
                React.createElement(TripToPrint.Commands, null,
                    React.createElement(TripToPrint.CommandHide, { onClick: () => { this.hide(); } }),
                    React.createElement(TripToPrint.CommandZoomIn, { onClick: () => { this.zoom(true); } }),
                    React.createElement(TripToPrint.CommandZoomOut, { onClick: () => { this.zoom(false); } })));
        }
        zoom(zoomIn) {
            let coef = 1 + PlacemarkImage.ZOOM_STEP * (zoomIn ? 1 : -1);
            let newZoom = coef * this.state.zoom;
            this.setState(TripToPrint.Utils.extend(this.state, { zoom: newZoom }));
        }
        onImageError(event) {
            let element = event.target;
            element.style.display = "none";
            console.log(`Image not loaded: ${element.getAttribute("src")}`);
        }
    }
    PlacemarkImage.DEFAULT_MAX_IMAGE_WIDTH = 170;
    PlacemarkImage.DEFAULT_MAX_IMAGE_HEIGHT = 120;
    PlacemarkImage.ZOOM_STEP = 0.2;
    TripToPrint.PlacemarkImage = PlacemarkImage;
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
            const doc = this.state.document;
            if (doc == null)
                return null;
            return React.createElement("div", null,
                React.createElement("h3", null, doc.title),
                (doc.description != null
                    ? React.createElement("p", { className: "doc-desc", dangerouslySetInnerHTML: { __html: doc.description } })
                    : null),
                this.renderSections());
        }
        renderSections() {
            const sections = this.state.document.sections;
            return sections.map((s, i) => React.createElement(TripToPrint.Section, { section: s, isFirst: i === 0 }));
        }
    }
    TripToPrint.Root = Root;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Section extends React.Component {
        render() {
            return React.createElement("div", { className: "folder" }, this.props.section.clusters.map((cluster, i) => React.createElement(TripToPrint.Cluster, { cluster: cluster, section: this.props.section, isFirst: this.props.isFirst && i === 0 })));
        }
    }
    TripToPrint.Section = Section;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class ThumbnailMap extends TripToPrint.Hideable {
        constructor(props) {
            super(props);
        }
        renderUnhidden() {
            const pm = this.props.placemark;
            const style = {};
            if (pm.iconPath.match(ThumbnailMap.FOURSQUARE_ICON_DOMAIN)) {
                style["background"] = "lightslategray";
            }
            return React.createElement("div", { className: "thumbnail-map-ctr" },
                React.createElement("img", { className: "map", src: pm.thumbnailFilePath }),
                React.createElement("img", { className: "icon", src: pm.iconPath, style: style }),
                React.createElement("div", { className: "ix" }, pm.index),
                React.createElement(TripToPrint.Commands, null,
                    React.createElement(TripToPrint.CommandHide, { onClick: () => { this.hide(); } })));
        }
    }
    ThumbnailMap.FOURSQUARE_ICON_DOMAIN = /4sqi\.net/;
    TripToPrint.ThumbnailMap = ThumbnailMap;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Utils {
        static extend(first, second) {
            let result = {};
            for (let id in first) {
                result[id] = first[id];
            }
            for (let id in second) {
                result[id] = second[id];
            }
            return result;
        }
    }
    TripToPrint.Utils = Utils;
})(TripToPrint || (TripToPrint = {}));
var TripToPrint;
(function (TripToPrint) {
    class Cluster extends React.Component {
        render() {
            const cluster = this.props.cluster;
            return React.createElement("div", null,
                React.createElement(TripToPrint.OverviewMap, { cluster: cluster, section: this.props.section, isFirst: this.props.isFirst }),
                cluster.isRoute ? this.renderRoute() : this.renderPoints());
        }
        renderRoute() {
            let cluster = this.props.cluster;
            return React.createElement("div", null, cluster.placemarks.map(pm => React.createElement(TripToPrint.Placemark, { placemark: pm, isInRouteCluster: cluster.isRoute })));
        }
        renderPoints() {
            let cluster = this.props.cluster;
            let totalImagesCount = cluster.placemarks.map(x => Math.max(1, x.images.length) - 1)
                .reduce((prev, curr) => prev + curr, 0);
            let meaningSizeOfCluster = cluster.placemarks.length + totalImagesCount;
            let placemarksInColumn1 = [];
            let placemarksInColumn2 = [];
            var incrementalColumnSize = 0;
            for (let i = 0; i < cluster.placemarks.length; i++) {
                let pm = cluster.placemarks[i];
                let pmElement = React.createElement(TripToPrint.Placemark, { placemark: pm, isInRouteCluster: cluster.isRoute });
                if (incrementalColumnSize >= meaningSizeOfCluster / 2) {
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
    TripToPrint.Cluster = Cluster;
})(TripToPrint || (TripToPrint = {}));
//# sourceMappingURL=combined.js.map