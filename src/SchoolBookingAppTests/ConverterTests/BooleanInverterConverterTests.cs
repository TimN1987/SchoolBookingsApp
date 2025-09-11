using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Converters;

namespace SchoolBookingAppTests.ConverterTests
{
    public class BooleanInverterConverterTests
    {
        private readonly BooleanInverterConverter _converter;

        public BooleanInverterConverterTests()
        {
            _converter = new BooleanInverterConverter();
        }

        //Constructor test

        /// <summary>
        /// Verifies that a non-null instance of <see cref="BooleanInverterConverter"/> is created when the constructor is 
        /// called.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var converter = new BooleanInverterConverter();

            //Assert
            Assert.NotNull(converter);
            Assert.IsType<BooleanInverterConverter>(converter);
        }

        //Convert tests.

        /// <summary>
        /// Verifies that a false boolean value is returned when a non-boolean input is passed to the <see 
        /// cref="BooleanInverterConverter.Convert"/> method.
        /// </summary>
        [Fact]
        public void Convert_NonBooleanInput_ReturnsFalse()
        {
            //Arrange
            object input = new object();

            //Act
            var result = _converter.Convert(input, null!, null!, null!);

            //Assert
            Assert.False((bool)result);
        }

        /// <summary>
        /// Verifies that a false boolean value is returned when a true boolean input is passed to the <see 
        /// cref="BooleanInverterConverter.Convert"/> method.
        /// </summary>
        [Fact]
        public void Convert_TrueInput_ReturnsFalse()
        {
            //Arrange
            bool input = true;

            //Act
            var result = _converter.Convert(input, null!, null!, null!);

            //Assert
            Assert.False((bool)result);
        }

        /// <summary>
        /// Verifies that a true boolean value is returned when a false boolean input is passed to the <see 
        /// cref="BooleanInverterConverter.Convert"/> method.
        /// </summary>
        [Fact]
        public void Convert_FalseInput_ReturnsTrue()
        {
            //Arrange
            bool input = false;

            //Act
            var result = _converter.Convert(input, null!, null!, null!);

            //Assert
            Assert.True((bool)result);
        }

        //ConvertBack tests.

        /// <summary>
        /// Verifies that a false boolean value is returned when a non-boolean input is passed to the <see 
        /// cref="BooleanInverterConverter.ConvertBack"/> method.
        /// </summary>
        [Fact]
        public void ConvertBack_NonBooleanInput_ReturnsFalse()
        {
            //Arrange
            object input = new object();

            //Act
            var result = _converter.ConvertBack(input, null!, null!, null!);

            //Assert
            Assert.False((bool)result);
        }

        /// <summary>
        /// Verifies that a false boolean value is returned when a true boolean input is passed to the <see 
        /// cref="BooleanInverterConverter.ConvertBack"/> method.
        /// </summary>
        [Fact]
        public void ConvertBack_TrueInput_ReturnsFalse()
        {
            //Arrange
            bool input = true;

            //Act
            var result = _converter.ConvertBack(input, null!, null!, null!);

            //Assert
            Assert.False((bool)result);
        }

        /// <summary>
        /// Verifies that a true boolean value is returned when a false boolean input is passed to the <see 
        /// cref="BooleanInverterConverter.ConvertBack"/> method.
        /// </summary>
        [Fact]
        public void ConvertBack_FalseInput_ReturnsTrue()
        {
            //Arrange
            bool input = false;

            //Act
            var result = _converter.ConvertBack(input, null!, null!, null!);

            //Assert
            Assert.True((bool)result);
        }
    }
}
