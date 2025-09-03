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
    public class PercentageToPieChartPathConverter : IValueConverter
    {
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
