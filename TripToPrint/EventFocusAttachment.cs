using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TripToPrint
{
    public class EventFocusAttachment
    {
        public static Control GetElementToFocus(ButtonBase button)
        {
            return (Control)button.GetValue(ElementToFocusProperty);
        }

        public static void SetElementToFocus(ButtonBase button, Control value)
        {
            button.SetValue(ElementToFocusProperty, value);
        }

        public static readonly DependencyProperty ElementToFocusProperty =
            DependencyProperty.RegisterAttached("ElementToFocus", typeof(Control),
            typeof(EventFocusAttachment), new UIPropertyMetadata(null, ElementToFocusPropertyChanged));

        public static void ElementToFocusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                button.Click += (s, args) => GetElementToFocus(button)?.Focus();
            }
        }
    }
}
