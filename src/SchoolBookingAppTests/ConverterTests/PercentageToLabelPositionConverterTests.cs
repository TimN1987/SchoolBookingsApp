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
        /// <summary>
        /// Verifies that a non-null instance of the <see cref="PercentageToLabelPositionConverter"/> class is created 
        /// when the parameterless constructor is called.
        /// </summary>
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
        [Theory]
        [InlineData(3.6, 4.2)]
        [InlineData(70.0, 'c')]
        [InlineData(32.4, 5)]
        [InlineData("convert", "XB")]
        [InlineData('c', "YU")]
        [InlineData(3, "YB")]
        [InlineData("convert", 7.0)]
        public void Convert_ValueAndParameterWrongType_ReturnsZero(
            object value, object parameter)
        {
            //Arrange & Act
            var labelPosition = _converter.Convert(value, null!, parameter, null!);
            var isDouble = double.TryParse(labelPosition.ToString(), out var doubleValue);

            //Assert
            Assert.True(isDouble);
            Assert.Equal(0d, doubleValue);
        }

        //ConvertBack test.
        /// <summary>
        /// Verifies that a <see cref="NotSupportedException"/> is thrown when the <see cref="PercentageToLabelPositionConverter
        /// .ConvertBack"/> method is called. Uses different types and values for the parameters to test that this 
        /// exception is always thrown avoiding any unexpected exceptions.
        /// </summary>
        /// <param name="value">The value passed from the view to the viewmodel.</param>
        /// <param name="targetType">The expected type of value passed from the view to the viewmodel, if given.</param>
        /// <param name="parameter">The converter parameter given in the view.</param>
        /// <param name="culture">The culture information used in any conversion.</param>
        [Theory]
        [MemberData(nameof(ConvertBackTestMemberData))]
        public void ConvertBack_AlwaysThrowsNotSupportedException(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Arrange, Act & Assert
            Assert.Throws<NotSupportedException>(() => _converter.ConvertBack(value, targetType, parameter, culture));
        }

        //MemberData
        /// <summary>
        /// Provides member data for the convert back test. Uses different values for each parameter to ensure a variety 
        /// of cases are tested where each case should thrown a <see cref="NotSupportedException"/>.
        /// </summary>
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
