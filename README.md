# Trip to Print
A WPF application which converts KMZ/KML files to handy easy-to-print PDF reports

# Usage
1. Open your [Google MyMap](https://www.google.com/maps/d/) in browser<br/>
![Google MyMap](Documentation/readme-1.png "Google MyMap")
2. Export your map to KMZ file<br/>
![Export to KMZ](Documentation/readme-2.png "Export to KMZ")<br/>
*NOTE: Alternatively you may share your map as public and use directly without downloading KMZ file*
3. Download the sources
4. Create your own [HERE API key](https://developer.here.com/plans?create=Evaluation)
5. Set up *HereApiAppId* and *HereApiAppCode* parameters in app.config file with values created in the previous step
6. Create your own [Foursquare API key](https://developer.foursquare.com/)
7. Set up *FoursquareApiClientId* and *FoursquareApiClientSecret* parameters in app.config file with values created in the previous step
8. Compile the sources with [Visual Studio 2017](https://www.visualstudio.com)
9. Run the Trip2Print application, select KMZ file or input map URL and start a report generation
10. Optionally you may select additional recommended places to add into the report:<br/>
![Exploring recommended places](Documentation/readme-3.png "PDF Sample")
11. Once created, you may adjust the report layout
12. Click the "generate report" button<br/>
13. Print out the generated PDF file and have fun!<br/>
![PDF Sample](Documentation/readme-4.png "PDF Sample")


# Features
* Overview maps for smartly generated groups of POIs
* Thumbnail maps for placemarks
* Pictures from Google MyMaps are supported
* Showing coordinates for every placemark
* Clickable coordinates for easier navigation from mobile devices
* Extra information for public places (such as restaurants, parks, etc.)

# To Do
* Make the output layout of placemarks more flexible
* Option to select a map source:
    1) HERE (implemented)
    2) Google Static Maps (https://developers.google.com/maps/documentation/static-maps/intro)
* Add integration with Yelp.com
* Add integration with Google Places

# Known issues
* Placemarks layout is still stupid
* KML's polygons are not supported

# Licenses

Icons for Settings Step are made by [Nick Roach](https://www.iconfinder.com/iconsets/circle-icons-1)
