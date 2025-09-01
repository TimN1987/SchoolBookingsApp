using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Converters;
using SchoolBookingAppTests.Mocks;

namespace SchoolBookingAppTests.ConverterTests
{
    public class ViewToEnabledConverterTests
    {
        private readonly ViewToEnabledConverter _converter = new ViewToEnabledConverter();

        /// <summary>
        /// Verifies that the <see cref="ViewToEnabledConverter"/> constructor successfully creates a new instance.
        /// </summary>
        [Fact]
        public void Constructor_NewInstance_InstanceCreatedSuccessfully()
        {
            //Arrange & Act
            var converter = new ViewToEnabledConverter();

            //Assert
            Assert.NotNull(converter);
            Assert.IsType<ViewToEnabledConverter>(converter);
        }

        /// <summary>
        /// Verifies that the <see cref="ViewToEnabledConverter.Convert"/> returns false if the current view matches the 
        /// converter parameter view type. Ensures that a button is disabled if it navigates to the current view.
        /// </summary>
        [StaFact]
        public void Convert_MatchingViews_ReturnsFalse()
        {
            //Arrange
            var currentView = new MockView();
            var converterParameter = typeof(MockView);

            //Act
            bool result = (bool)_converter.Convert(currentView, null!, converterParameter, null!);

            //Assert
            Assert.False(result);
        }

        /// <summary>
        /// Verifies that the <see cref="ViewToEnabledConverter.Convert"/> returns true if the current view does not match 
        /// the converter parameter view type. Ensures that a button is enabled if it navigates to a different view.
        /// </summary>
        [StaFact]
        public void Convert_DifferentViews_ReturnsTrue()
        {
            //Arrange
            var currentView = new MockView();
            var converterParameter = typeof(AlternativeMockView);

            //Act
            bool result = (bool)_converter.Convert(currentView, null!, converterParameter, null!);

            //Assert
            Assert.True(result);
        }
    }
}
