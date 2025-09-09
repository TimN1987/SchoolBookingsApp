using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.MVVM.Viewmodel;

namespace SchoolBookingAppTests.ViewModelTests
{
    public class ViewBookingViewModelTests
    {
        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly ViewBookingViewModel _viewModel;

        public ViewBookingViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();

            _viewModel = new ViewBookingViewModel(_eventAggregatorMock.Object);
        }

        //Constructor tests.

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when the event aggregator is null. Ensures that 
        /// a valid event aggregator instance is passed to the <see cref="ViewBookingViewModel"/> to allow subscription 
        /// to necessary events.
        /// </summary>
        [Fact]
        public void Constructor_NullEventAggregator_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ViewBookingViewModel(null!));
        }
    }
}
