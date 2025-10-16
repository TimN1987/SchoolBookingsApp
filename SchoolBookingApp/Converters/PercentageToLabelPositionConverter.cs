using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SchoolBookingApp.Converters;

/// <summary>
/// A value converter that converts a percentage value to a position for a pie chart label. Used to ensure correct 
/// positioning of labels for the booked and unbooked segments of the pie chart.
/// </summary>
public class PercentageToLabelPositionConverter : IValueConverter
{
    /// <summary>
    /// Converts a percentage value to a position for a pie chart label. Uses the parameter to determine which segment 
    /// of the pie chart the label is for and whether to calculate the X or Y position. Used to position labels for 
    /// the booked and unbooked segments of the pie chart.
    /// </summary>
    /// <param name="value">The percentage represented by the segment to be labelled.</param>
    /// <param name="parameter">The converter parameter given in the view. Indicates which segment is being 
    /// labelled and whether the X or Y position is required.</param>
    /// <returns>A <see langword="double"/> value giving the X or Y value to set the <c>Top</c> or <c>Left</c> 
    /// position for the label.</returns>
    /// <remarks>The converter parameter should consist of two characters: X or Y to indicate which axis is being set, 
    /// and B or U to indicate whether the booked or unbooked segment is being labelled.</remarks>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percentage && parameter is string labelInfo)
        {
            double angle = percentage * 3.6 / 2;
            double angleRad = angle * Math.PI / 180.0;
            bool isYPosition = labelInfo[0] == 'Y';
            bool isBookedLabel = labelInfo[1] == 'B';

            if (isYPosition)
                return 100 - 50 * Math.Cos(angleRad);
            if (isBookedLabel)
                return 100 - 50 * Math.Sin(angleRad);
            return 100 + 50 * Math.Sin(angleRad);
        }

        return 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
