using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SchoolBookingApp.MVVM.Converters;

namespace SchoolBookingAppTests.ConverterTests
{
    public class BooleanToVisibilityConverterTests
    {
        private readonly BooleanToVisibilityConverter _converter;

        public BooleanToVisibilityConverterTests()
        {
            _converter = new BooleanToVisibilityConverter();
        }

        //Constructor test.

        /// <summary>
        /// Verifies that a non-null instance of <see cref="BooleanToVisibilityConverter"/> is created when the constructor 
        /// is called.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var converter = new BooleanToVisibilityConverter();

            //Assert
            Assert.NotNull(converter);
            Assert.IsType<BooleanToVisibilityConverter>(converter);
        }

        //Convert tests.

        /// <summary>
        /// Verifies that a <c>true</c> input is converted into a <see cref="Visibility.Visible"/> state.
        /// </summary>
        [Fact]
        public void Convert_TrueInput_ReturnsVisible()
        {
            // Arrange
            bool input = true;

            // Act
            var result = _converter.Convert(input, null!, null!, null!);

            // Assert
            Assert.Equal(Visibility.Visible, result);
        }

        /// <summary>
        /// Verifies that a <c>false</c> input is converted into a <see cref="Visibility.Collapsed"/> state.
        /// </summary>
        [Fact]
        public void Convert_FalseInput_ReturnsCollapsed()
        {
            //Arrange
            bool input = false;

            //Act
            var result = _converter.Convert(input, null!, null!, null!);

            //Assert
            Assert.Equal(Visibility.Collapsed, result);
        }

        /// <summary>
        /// Verifies that the default value of <see cref="Visibility.Collapsed"/> is returned if a non-boolean input is 
        /// passed to the <see cref="BooleanToVisibilityConverter.Convert"/> method.
        /// </summary>
        [Fact]
        public void Convert_NonBooleanInput_ReturnsCollapsed()
        {
            //Arrange
            object input = new object();

            //Act
            var result = _converter.Convert(input, null!, null!, null!);

            //Assert
            Assert.Equal(Visibility.Collapsed, result);
        }

        //ConvertBack tests.

        /// <summary>
        /// Verifies that <see langword="true"/> is returned when <see cref="Visibility.Visible"/> is passed to the <see 
        /// cref="BooleanToVisibilityConverter.ConvertBack"/> method.
        /// </summary>
        [Fact]
        public void ConvertBack_VisibilityVisibleInput_ReturnsTrue()
        {
            //Arrange
            Visibility input = Visibility.Visible;

            //Act
            var result = _converter.ConvertBack(input, null!, null!, null!);

            //Assert
            Assert.True((bool)result);
        }

        /// <summary>
        /// Verifies that <see langword="false"/> is returned when <see cref="Visibility.Collapsed"/> is passed to the <see 
        /// cref="BooleanToVisibilityConverter.ConvertBack"/> method.
        /// </summary>
        [Fact]
        public void ConvertBack_VisibilityCollapsedInput_ReturnsFalse()
        {
            //Arrange
            Visibility input = Visibility.Collapsed;

            //Act
            var result = _converter.ConvertBack(input, null!, null!, null!);

            //Assert
            Assert.False((bool)result);
        }

        /// <summary>
        /// Verifies that <see langword="false"/> is returned when a non-Visibility parameter is passed to the <see 
        /// cref="BooleanToVisibilityConverter.ConvertBack"/> method.
        /// </summary>
        [Fact]
        public void ConvertBack_NonVisibilityInput_ReturnsFalse()
        {
            //Arrange
            object input = new object();

            //Act
            var result = _converter.ConvertBack(input, null!, null!, null!);

            //Assert
            Assert.False((bool)result);
        }
    }
}
