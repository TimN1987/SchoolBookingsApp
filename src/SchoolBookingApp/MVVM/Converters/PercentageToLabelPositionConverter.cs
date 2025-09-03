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
    public class PercentageToLabelPositionConverter : IValueConverter
    {
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

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
