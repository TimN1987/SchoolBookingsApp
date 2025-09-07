using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Converters;

namespace SchoolBookingAppTests.ConverterTests
{
    public class PercentageToLabelPositionConverterTests
    {
        private PercentageToLabelPositionConverter _converter;

        public PercentageToLabelPositionConverterTests()
        {
            _converter = new PercentageToLabelPositionConverter();
        }

        //Constructor test.
        [Fact]
        public void Constructor_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var converter = new PercentageToLabelPositionConverter();

            //Assert
            Assert.NotNull(converter);
            Assert.IsType<PercentageToLabelPositionConverter>(converter);
        }

        //Convert tests.

        //ConvertBack test.
        [Theory]
        [MemberData(nameof(ConvertBackTestMemberData))]
        public void ConvertBack_AlwaysThrowsNotSupportedException(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Arrange, Act & Assert
            Assert.Throws<NotSupportedException>(() => _converter.ConvertBack(value, targetType, parameter, culture));
        }

        //MemberData
        public static IEnumerable<object?[]> ConvertBackTestMemberData()
        {
            yield return new object?[] { null, null, null, null };
            yield return new object?[] { 50.0, typeof(double), "YB", CultureInfo.InvariantCulture };
            yield return new object?[] { "convert", typeof(string), 17, CultureInfo.CurrentCulture };
            yield return new object?[] { 100, typeof(int), 'c', CultureInfo.InvariantCulture };
            yield return new object?[] { 'b', typeof(char), 36.0, CultureInfo.CurrentUICulture };
            yield return new object?[] { 150.0, typeof(DateTime), 3, CultureInfo.InvariantCulture };
            yield return new object?[] { "convert", null, DateTime.Now, CultureInfo.CurrentUICulture };
            yield return new object?[] { 50.0, typeof(string), null, CultureInfo.CurrentUICulture };
            yield return new object?[] { DateTime.Now, typeof(long), 'e', null };
        }
    }
}
