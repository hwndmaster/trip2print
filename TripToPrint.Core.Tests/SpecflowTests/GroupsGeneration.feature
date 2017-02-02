Feature: Groups Generation
	Test different scenarios of dividing placemarks by groups

Scenario: Scenario for two groups: 4 + 3 placemarks
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
