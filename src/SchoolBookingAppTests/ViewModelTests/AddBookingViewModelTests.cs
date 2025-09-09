using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Viewmodel;
using SchoolBookingApp.MVVM.Services;

namespace SchoolBookingAppTests.ViewModelTests
{
    public class AddBookingViewModelTests
    {
        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly Mock<IBookingManager> _bookingManagerMock;
        private readonly Mock<DisplayBookingEvent> _displayBookingEventMock;
        private readonly AddBookingViewModel _viewModel;

        public AddBookingViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _bookingManagerMock = new Mock<IBookingManager>();
            _displayBookingEventMock = new Mock<DisplayBookingEvent>();

            _eventAggregatorMock
                .Setup(x => x.GetEvent<DisplayBookingEvent>())
                .Returns(_displayBookingEventMock.Object);

            _viewModel = new AddBookingViewModel(_eventAggregatorMock.Object, _bookingManagerMock.Object);
        }

        //Constructor tests.

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when the event aggregator is null. Ensures that 
        /// a valid event aggregator instance is passed to the <see cref="AddBookingViewModel"/> to allow subscription 
        /// to necessary events.
        /// </summary>
        [Fact]
        public void Constructor_NullEventAggregator_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddBookingViewModel(null!, _bookingManagerMock.Object));
        }

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when the booking manager is null. Ensures that 
        /// a valid booking manager instance is passed to the <see cref="AddBookingViewModel"/> to allow access to the 
        /// bookings data.
        /// </summary>
        [Fact]
        public void Constructor_NullBookingManager_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddBookingViewModel(_eventAggregatorMock.Object, null!));
        }

        /// <summary>
        /// Verifies that a valid instance of <see cref="AddBookingViewModel"/> is created when a valid parameters are 
        /// provided. Ensures that the view model can be instantiated correctly and is ready to subscribe to events and 
        /// access data.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var viewModel = new AddBookingViewModel(_eventAggregatorMock.Object, _bookingManagerMock.Object);

            //Assert
            Assert.NotNull(viewModel);
            Assert.IsType<AddBookingViewModel>(viewModel);
        }

        //EventAggregator tests.

        [Theory]
        [MemberData(nameof(BookingsMemberData))]
        public void AddBookingViewModel_DisplayBookingEventPublished_PublishedBookingSetInViewModel(Booking booking)
        {
            //Arrange
            var eventAggregator = new EventAggregator();
            var viewModel = new AddBookingViewModel(eventAggregator, _bookingManagerMock.Object);

            //Act
            eventAggregator.GetEvent<DisplayBookingEvent>().Publish(booking);

            //Assert
            Assert.Equal(booking, viewModel.Booking);
        }

        //MemberData

        /// <summary>
        /// Provides member data returning instances of the <see cref="Booking"/> <see langword="struct"/> to use for 
        /// testing with different bookings.
        /// </summary>
        public static IEnumerable<object[]> BookingsMemberData()
        {
            yield return new object[] { new Booking(1, "John", "Smith", "2025-11-19", "14:00") };
            yield return new object[] { new Booking(2, "Jane", "Doe", new DateTime(2025, 09, 14), new TimeSpan(11, 0, 0)) };
            yield return new object[] { new Booking(3, "2025-10-13", "13:50") };
            yield return new object[] { new Booking(4, new DateTime(2025, 11, 17, 20, 0, 0)) };
        }
    }
}
