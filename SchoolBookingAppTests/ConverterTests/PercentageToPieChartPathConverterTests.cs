using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SchoolBookingApp.Converters;

namespace SchoolBookingAppTests.ConverterTests;

public class PercentageToPieChartPathConverterTests
{
    private PercentageToPieChartPathConverter _converter;

    public PercentageToPieChartPathConverterTests()
    {
        _converter = new PercentageToPieChartPathConverter();
    }

    //Constructor test.

    /// <summary>
    /// Verifies that a non-null instance of <see cref="PercentageToPieChartPathConverter"/> is created successfully. 
    /// Ensures that the constructor functions as expected.
    /// </summary>
    [Fact]
    public void Constructor_InstanceCreatedSuccessfully()
    {
        //Arrange & Act
        var converter = new PercentageToPieChartPathConverter();

        //Assert
        Assert.NotNull(converter);
        Assert.IsType<PercentageToPieChartPathConverter>(converter);
    }

    //Convert tests.

    /// <summary>
    /// Verifies that a empty geometry is returned when the value, parameter or both are invalid types for the <see 
    /// cref="PercentageToPieChartPathConverter.Convert"/> method. Ensures that no segment is drawn when the input 
    /// types are invalid
    /// </summary>
    /// <param name="value">The value passed from the view to the viewmodel.</param>
    /// <param name="parameter">The converter parameter given in the view.</param>
    [Theory]
    [InlineData(3.6, 4.2)]
    [InlineData(70.0, 'c')]
    [InlineData(32.4, 5)]
    [InlineData("convert", "XB")]
    [InlineData('c', "YU")]
    [InlineData(3, "YB")]
    [InlineData("convert", 7.0)]
    public void Convert_ValueAndParameterWrongType_ReturnsEmptyGeometry(
        object value, object parameter)
    {
        //Arrange & Act
        var result = _converter.Convert(value, null!, parameter, null!);

        //Assert
        Assert.IsAssignableFrom<Geometry>(result);
        Assert.Equal(string.Empty, result.ToString());
    }

    /// <summary>
    /// Verifies that the <paramref name="expectedPath"/> is returned when a given <paramref name="value"/> 
    /// and <paramref name="parameter"/> are passed as arguments to the <see cref="PercentageToPieChartPathnConverter
    /// .Convert"/> method. Ensures that the segment size is correct.
    /// </summary>
    /// <param name="value">The value passed from the view to the viewmodel.</param>
    /// <param name="parameter">The converter parameter given in the view.</param>
    /// <param name="expectedPath">The expected path string used to draw the pie chart segment.</param>
    [Theory]
    [MemberData(nameof(ConvertTestMemberData))]

    public void Convert_CorrectTypeValueAndParameter_ExpectedPathReturned(
        object value, object parameter, string expectedPath)
    {
        //Arrange & Act
        var result = _converter.Convert(value, null!, parameter, null!);

        //Act
        Assert.IsAssignableFrom<Geometry>(result);
        Assert.Equal(expectedPath, result.ToString());
    }

    //ConvertBack test.
    /// <summary>
    /// Verifies that a <see cref="NotSupportedException"/> is thrown when the <see cref="PercentageToPieChartPathConverter
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

    /// <summary>
    /// Provides member data for the convert test. Each case includes a percentage value, a boolean parameter and the 
    /// expected path string to be returned. Focuses on key edge cases to avoid issues with floating point precision.
    /// </summary>
    public static IEnumerable<object[]> ConvertTestMemberData()
    {
        yield return new object[] { 0d, true, "M100,100L100,10A90,90,0,0,0,100,10L100,100z" };
        yield return new object[] { 0d, false, "M100,100L100,10A90,90,0,0,1,100,10L100,100z" };
        yield return new object[] { 100d, true, "M100,100L100,10A90,90,0,1,0,100,190A90,90,0,1,0,100,10z" };
        yield return new object[] { 100d, false, "M100,100L100,10A90,90,0,1,0,100,190A90,90,0,1,0,100,10z" };
    }
}
