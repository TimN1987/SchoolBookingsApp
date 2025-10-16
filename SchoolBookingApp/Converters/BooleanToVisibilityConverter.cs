using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace SchoolBookingApp.Converters;

/// <summary>
/// A value converter that converts a boolean value to a Visibility state. Used in XAML bindings to show or hide UI 
/// elements based on boolean properties.
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to a Visibility state. Used in XAML bindings to show or hide UI elements based on the 
    /// value of a boolean property.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a Visibility state back to a boolean value. Used in XAML bindings to retrieve the boolean value from 
    /// the Visibility state of a UI element.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
