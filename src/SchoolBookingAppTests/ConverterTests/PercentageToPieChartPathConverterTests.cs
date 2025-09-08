using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Converters;

namespace SchoolBookingAppTests.ConverterTests
{
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
    }
}
