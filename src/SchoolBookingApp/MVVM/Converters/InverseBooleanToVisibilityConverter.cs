using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace SchoolBookingApp.MVVM.Converters
{
    /// <summary>
    /// A class for setting the visibility of a control based on the inverse of a boolean value.
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Sets the visibility of a control based on the inverse of a boolean value.
        /// </summary>
        /// <param name="value">A boolean value to be converter into a Visibility.</param>
        /// <returns>The visibility status for the selected control.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
                return booleanValue ? Visibility.Collapsed : Visibility.Visible;

            return Visibility.Visible;
        }

        /// <summary>
        /// Returns a boolean value based on the Visibility status.
        /// </summary>
        /// <param name="value">The Visibility status to be converted to a boolean value.</param>
        /// <returns>The boolean value derived from the Visibility status.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility != Visibility.Visible;

            return true;
        }
    }
}
