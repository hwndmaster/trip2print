using System;
using System.Windows;
using TripToPrint.Core.Models.Venues;
using TripToPrint.ViewModels;

namespace TripToPrint.AttachedProperties
{
    public static class VenueDataSource
    {
        public static readonly DependencyProperty VenueProperty =
            DependencyProperty.RegisterAttached("Venue", typeof(VenueBase), typeof(VenueDataSource),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    OnVenueChanged));

        public static VenueBase GetVenue(FrameworkElement dp)
        {
            return (VenueBase)dp.GetValue(VenueProperty);
        }

        public static void SetVenue(FrameworkElement dp, VenueBase value)
        {
            dp.SetValue(VenueProperty, value);
        }

        private static void OnVenueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            if (element == null)
                return;

            var newValue = (VenueBase)e.NewValue;

            // TODO: Cover with unit tests
            if (newValue is FoursquareVenue)
            {
                d.SetValue(FrameworkElement.DataContextProperty, new FoursquareVenueViewModel {
                    Venue = newValue
                });
            }
            else if (newValue is HereVenue)
            {
                d.SetValue(FrameworkElement.DataContextProperty, new HereVenueViewModel {
                    Venue = newValue
                });
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
