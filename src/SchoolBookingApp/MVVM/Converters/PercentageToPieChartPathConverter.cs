using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SchoolBookingApp.MVVM.Converters
{
    /// <summary>
    /// Converts a percentage value into a pie chart segment path represented as a <see cref="Geometry"/> object.
    /// </summary>
    /// <remarks>This converter generates a pie chart segment based on the provided percentage value. The
    /// segment is drawn as a path, starting from the top of the circle (12 o'clock position) and extending clockwise.
    /// The converter supports an optional parameter to determine whether the segment represents a "booked" or
    /// "available" portion, which affects the sweep direction of the arc.</remarks>
    public class PercentageToPieChartPathConverter : IValueConverter
    {
        /// <summary>
        /// Converts a percentage value into a path representing a pie chart segment. Used to draw the booked and unbooked 
        /// segments of the pie chart for the dashboard view.
        /// </summary>
        /// <param name="value">The percentage value, represented as a <see cref="double"/>. Must be between 0 and 100.</param>
        /// <param name="parameter">A <see cref="bool"/> indicating whether the segment represents a "booked" segment. If <see
        /// langword="true"/>, the segment is drawn in a clockwise direction; otherwise, it is drawn counterclockwise.</param>
        /// <returns>A <see cref="Geometry"/> object representing the circular segment corresponding to the percentage value.
        /// Returns <see cref="Geometry.Empty"/> if the input value is not a valid percentage or the parameter is not a
        /// <see cref="bool"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage && parameter is bool isBookedSegment)
            {
                //Avoids attempting to use arc to draw a full circle for the 100% special case.
                if (percentage == 100)
                    return Geometry.Parse(
                        "M 100,100 L 100,10 A 90,90 0 1 0 100,190 A 90,90 0 1 0 100,10 Z"
                        );
                
                double angle = percentage * 3.6;
                double angleRad = angle * Math.PI / 180.0;
                int largeAngleFlag = percentage >= 50 ? 1 : 0;
                int sweepDirectionFlag = isBookedSegment ? 0 : 1;
                double endX = isBookedSegment ? 100 - 90 * Math.Sin(angleRad) : 100 + 90 * Math.Sin(angleRad);
                double endY = 100 - 90 * Math.Cos(angleRad);

                return Geometry.Parse(
                    $"M 100,100 L 100,10 A 90,90 0 {largeAngleFlag} {sweepDirectionFlag} {endX},{endY} L 100,100 Z"
                    );
            }

            return Geometry.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
