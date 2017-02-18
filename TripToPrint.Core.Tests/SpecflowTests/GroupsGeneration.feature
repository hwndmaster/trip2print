Feature: Groups Generation
	Test different scenarios of dividing placemarks by groups

Scenario: A single group: 1 placemark
	Given I have these placemarks in my folder:
         | Longitude | Latitude | Name |
         | 45.91142  | 10.8345  | 1    |
	Then these placemarks will be assigned to the following groups:
         | Group Index | Name |
         | 0           | 1    |

Scenario: A single group: 6 placemark
	Given I have these placemarks in my folder:
         | Longitude | Latitude  | Name |
         | 28.55633  | -16.3357  | 1    |
         | 28.54533  | -16.22867 | 2    |
         | 28.50945  | -16.18511 | 3    |
         | 28.48714  | -16.31343 | 4    |
         | 28.48649  | -16.31535 | 5    |
         | 28.49828  | -16.37672 | 6    |
	Then these placemarks will be assigned to the following groups:
         | Group Index | Name |
         | 0           | 1    |
         | 0           | 2    |
         | 0           | 3    |
         | 0           | 4    |
         | 0           | 5    |
         | 0           | 6    |

Scenario: For two groups: 4 + 3 placemarks
	Given I have these placemarks in my folder:
        | Longitude | Latitude  | Name                                 |
        | 28.0552   | -16.73494 | Restaurante La Gula @ Los Cristianos |
        | 28.05622  | -16.72488 | El Aderno @ Los Cristianos           |
        | 28.05273  | -16.71508 | Panaria @ Los Cristianos             |
        | 28.05145  | -16.72196 | El Pincho @ Los Cristianos           |
        | 28.03397  | -16.6412  | La Dulce Emilia @ Guargacho          |
        | 28.11924  | -16.67092 | Restaurante El Chamo @ La Escalona   |
        | 28.12183  | -16.7406  | Tandem Paragliding @ Adeje           |
	Then these placemarks will be assigned to the following groups:
        | Group Index | Name                                 |
        | 0           | Restaurante La Gula @ Los Cristianos |
        | 0           | El Aderno @ Los Cristianos           |
        | 0           | Panaria @ Los Cristianos             |
        | 0           | El Pincho @ Los Cristianos           |
        | 1           | La Dulce Emilia @ Guargacho          |
        | 1           | Restaurante El Chamo @ La Escalona   |
        | 1           | Tandem Paragliding @ Adeje           |

@unclear		
Scenario: For two groups: 8 + 1 placemarks
	Given I have these placemarks in my folder:
         | Longitude | Latitude  | Name |
         | 28.47526  | -16.41868 | 1    |
         | 28.46999  | -16.38628 | 2    |
         | 28.49828  | -16.37672 | 3    |
         | 28.48451  | -16.34342 | 4    |
         | 28.48649  | -16.31535 | 5    |
         | 28.48714  | -16.31343 | 6    |
         | 28.46661  | -16.31083 | 7    |
         | 28.46139  | -16.30462 | 8    |
         | 28.54533  | -16.22867 | 9    |
	Then these placemarks will be assigned to the following groups:
         | Group Index | Name |
         | 1           | 1    |
         | 1           | 2    |
         | 1           | 3    |
         | 0           | 4    |
         | 0           | 5    |
         | 0           | 6    |
         | 0           | 7    |
         | 0           | 8    |
         | 0           | 9    |
		 #| 0           | 1    |
         #| 0           | 2    |
         #| 0           | 3    |
         #| 0           | 4    |
         #| 0           | 5    |
         #| 0           | 6    |
         #| 0           | 7    |
         #| 0           | 8    |
         #| 1           | 9    |

@unclear
Scenario: For three group: 4 + 5 + 1
	Given I have these placemarks in my folder:
         | Longitude | Latitude  | Name |
		 | 28.47526  | -16.41868 | 1    |
         | 28.46999  | -16.38628 | 2    |
         | 28.49828  | -16.37672 | 3    |
         | 28.48451  | -16.34342 | 4    |
         | 28.48649  | -16.31535 | 5    |
         | 28.48714  | -16.31343 | 6    |
         | 28.46661  | -16.31083 | 7    |
         | 28.46139  | -16.30462 | 8    |
         | 28.54533  | -16.22867 | 9    |
		 | 28.49947  | -16.40842 | 10   |
	Then these placemarks will be assigned to the following groups:
         | Group Index | Name |
         | 1           | 1    |
         | 1           | 2    |
         | 1           | 3    |
         | 0           | 4    |
         | 0           | 5    |
         | 0           | 6    |
         | 0           | 7    |
         | 0           | 8    |
         | 0           | 9    |
         | 1           | 10   |
	     #| 1           | 1    |
         #| 1           | 2    |
         #| 1           | 3    |
         #| 0           | 4    |
         #| 0           | 5    |
         #| 0           | 6    |
         #| 0           | 7    |
         #| 0           | 8    |
         #| 2           | 9    |
         #| 1           | 10   |
