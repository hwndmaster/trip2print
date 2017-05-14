
 
 

 

/// <reference path="Enums.ts" />

declare namespace Interfaces {
	interface IDiscoveredPlaceDto {
		address: string;
		averageRating: number;
		contactPhone: string;
		openingHours: string;
		title: string;
		website: string;
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
		coordinates: string[];
		description: string;
		discoveredData: Interfaces.IDiscoveredPlaceDto;
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
}


