
 
 

 

/// <reference path="Enums.ts" />

declare namespace Interfaces {
	interface IFoursquareVenueDto extends Interfaces.IVenueBaseDto {
		likesCount: number;
		maxRating: number;
		photoUrls: string[];
		priceLevel: string;
		rating: number;
		ratingColor: string;
		remainingPriceLevel: string;
		tags: string[];
		tips: Interfaces.IFoursquareVenueTipDto[];
	}
	interface IFoursquareVenueTipDto {
		agreeCount: number;
		disagreeCount: number;
		likes: number;
		message: string;
	}
	interface IHereVenueDto extends Interfaces.IVenueBaseDto {
		wikipediaContent: string;
	}
	interface IMooiClusterDto {
		id: string;
		isRoute: boolean;
		overviewMapFilePath: string;
		placemarks: Interfaces.IMooiPlacemarkDto[];
	}
	interface IMooiDocumentDto {
		description: string;
		sections: Interfaces.IMooiSectionDto[];
		title: string;
	}
	interface IMooiPlacemarkDto {
		attachedVenues: Interfaces.IVenueBaseDto[];
		coordinates: string[];
		description: string;
		distance: string;
		iconPath: string;
		id: string;
		images: string[];
		index: number;
		isShape: boolean;
		name: string;
		thumbnailFilePath: string;
	}
	interface IMooiSectionDto {
		clusters: Interfaces.IMooiClusterDto[];
		name: string;
	}
	interface IVenueBaseDto {
		address: string;
		category: string;
		contactPhone: string;
		openingHours: string;
		sourceType: string;
		title: string;
		website: string;
	}
}


