using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace SchoolBookingApp.Converters;

/// <summary>
/// A class for converting dates between an <see langword="int"/> representation and a <see cref="DateTime"/>. Used to 
/// ensure clear date display in the UI.
/// </summary>
public class IntToDateTimeConverter : IValueConverter
{
    /// <summary>
    /// Converts an int32 date with format yyyyMMdd into a DateTime object. Used to ensure that dates are easily 
    /// displayed in the UI when stored in <see langword="int"/> form in the database.
    /// </summary>
    /// <param name="value">The date as an <see langword="int"/> in the format yyyyMMdd.</param>
    /// <returns>The given date as a <see cref="DateTime"/> object. If the input is invalid, returns the min value for 
    /// a <see cref="DateTime"/>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int date)
        {
            int year = date/10000;
            int month = date % 10000 / 100;
            int day = date % 100;

            bool validDate = year >= 1
                && year <= 9999 && month >= 1 && month <= 12
                && day >= 1 && day <= DateTime.DaysInMonth(year, month);

            return validDate ? new DateTime(year, month, day) : DependencyProperty.UnsetValue;
        }

        return DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> date into its <see langword="int"/> representation in the format yyyyMMdd.
    /// </summary>
    /// <param name="value">A <see cref="DateTime"/> representing the date to be converted.</param>
    /// <returns>The date as an <see langword="int"/> in the format yyyyMMdd.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            return date.Year * 10000 + date.Month * 100 + date.Day;
        }

        return DependencyProperty.UnsetValue;
    }
}
