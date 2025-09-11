using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace SchoolBookingApp.MVVM.Converters
{
    /// <summary>
    /// A class to invert a boolean value. Enables simple management of multiple states that should be mutually exclusive, 
    /// e.g. when one view is enabled another button is disabled, allowing both to bind to the same boolean value.
    /// </summary>
    public class BooleanInverterConverter : IValueConverter
    {
        /// <summary>
        /// Inverts a boolean value when passed from the viewmodel to the view.
        /// </summary>
        /// <param name="value">A boolean value to be inverted when passed from the viewmodel to the view.</param>
        /// <returns><c>true</c> if the input value is <c>false</c>. <c>false</c> if the input value is <c>true</c>.</returns>
        public object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
                return !booleanValue;
            return false;
        }

        /// <summary>
        /// Inverts a boolean value when passed from the view to the viewmodel.
        /// </summary>
        /// <param name="value">A boolean value to be inverted when passed from the view to the viewmodel.</param>
        /// <returns><c>true</c> if the input value is <c>false</c>. <c>false</c> if the input value is <c>true</c>.</returns>
        public object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
                return !booleanValue;
            return false;
        }
    }
}
