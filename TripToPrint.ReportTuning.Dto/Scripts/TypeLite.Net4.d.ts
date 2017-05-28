
 
 

 

/// <reference path="Enums.ts" />

declare namespace Interfaces {
	interface IFoursquareVenueDto extends Interfaces.IVenueBaseDto {
		distance: number;
		likesCount: number;
		maxRating: number;
		photoUrls: string[];
		phrases: string[];
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
	interface IMooiDocumentDto {
		description: string;
		sections: Interfaces.IMooiSectionDto[];
		title: string;
	}
	interface IMooiGroupDto {
		id: string;
		isRoute: boolean;
		overviewMapFilePath: string;
		placemarks: Interfaces.IMooiPlacemarkDto[];
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
		groups: Interfaces.IMooiGroupDto[];
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


