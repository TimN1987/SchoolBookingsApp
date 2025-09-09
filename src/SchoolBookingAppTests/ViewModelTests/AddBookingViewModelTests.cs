using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Viewmodel;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Struct;

namespace SchoolBookingAppTests.ViewModelTests
{
    public class AddBookingViewModelTests
    {
        //Constant error messages
        private const string NoBookingDataMessage = "Complete all fields before adding booking.";

        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly Mock<IBookingManager> _bookingManagerMock;
        private readonly Mock<IReadOperationService> _readOperationServiceMock;
        private readonly Mock<ICreateOperationService> _createOperationServiceMock;
        private readonly Mock<IUpdateOperationService> _updateOperationServiceMock;
        private readonly Mock<IDeleteOperationService> _deleteOperationServiceMock;
        private readonly Mock<DisplayBookingEvent> _displayBookingEventMock;
        private readonly AddBookingViewModel _viewModel;
        private readonly Student _testStudent;
        private readonly Booking _testBooking;
        private readonly Booking _invalidBooking;

        public AddBookingViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _bookingManagerMock = new Mock<IBookingManager>();
            _readOperationServiceMock = new Mock<IReadOperationService>();
            _createOperationServiceMock = new Mock<ICreateOperationService>();
            _updateOperationServiceMock = new Mock<IUpdateOperationService>();
            _deleteOperationServiceMock = new Mock<IDeleteOperationService>();

            _displayBookingEventMock = new Mock<DisplayBookingEvent>();
            _testStudent = new Student(
                1, "John", "Doe", 20191110, "5B", [], new StudentDataRecord(), new MeetingCommentsRecord());

            _eventAggregatorMock
                .Setup(x => x.GetEvent<DisplayBookingEvent>())
                .Returns(_displayBookingEventMock.Object);
            _bookingManagerMock.Setup(x => x.ListBookings())
                .Returns(Task.FromResult<List<Booking>>([]));
            _readOperationServiceMock.Setup(x => x.GetStudentData(It.IsAny<int>()))
                .Returns(Task.FromResult(_testStudent));

            _viewModel = new AddBookingViewModel(
                _eventAggregatorMock.Object, 
                _bookingManagerMock.Object, 
                _readOperationServiceMock.Object, 
                _createOperationServiceMock.Object, 
                _updateOperationServiceMock.Object, 
                _deleteOperationServiceMock.Object);
            _testBooking = new Booking(1, "John", "Doe", "2025-12-25", "12:00");
            _invalidBooking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        //Constructor tests.

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when the one or more of the required dependencies 
        /// are <see langword="null"/> when instantiating the <see cref="AddBookingViewModel"/>. Ensures that the view model 
        /// cannot be created without its necessary dependencies, which would lead to runtime errors.
        /// </summary>
        [Theory]
        [MemberData(nameof(ConstructorParametersMemberData))]
        public void Constructor_NullEventAggregator_ThrowsArgumentNullException(
            IEventAggregator eventAggregator,
            IBookingManager bookingManager, 
            IReadOperationService readOperationService,
            ICreateOperationService createOperationService, 
            IUpdateOperationService updateOperationService, 
            IDeleteOperationService deleteOperationService)
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddBookingViewModel(
                eventAggregator, 
                bookingManager, 
                readOperationService, 
                createOperationService, 
                updateOperationService, 
                deleteOperationService));
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
            var viewModel = new AddBookingViewModel(
                _eventAggregatorMock.Object, 
                _bookingManagerMock.Object, 
                _readOperationServiceMock.Object, 
                _createOperationServiceMock.Object, 
                _updateOperationServiceMock.Object, 
                _deleteOperationServiceMock.Object);

            //Assert
            Assert.NotNull(viewModel);
            Assert.IsType<AddBookingViewModel>(viewModel);
        }

        //EventAggregator tests.

        /// <summary>
        /// Verifies that the expected <see cref="Booking"/> is set in the <see cref="AddBookingViewModel.Booking"/> 
        /// property when a <see cref="DisplayBookingEvent"/> is published with that booking. Ensures that the view model 
        /// responds correctly to the event and updates its state accordingly.
        /// </summary>
        /// <param name="booking">An instance of the <see cref="Booking"/> <see langword="struct"/> to be published by 
        /// the <see cref="DisplayBookingEvent"/>. It should be set as the <see cref="AddBookingViewModel.Booking"/> 
        /// property.</param>
        [Theory]
        [MemberData(nameof(BookingsMemberData))]
        public void AddBookingViewModel_DisplayBookingEventPublished_PublishedBookingSetInViewModel(Booking booking)
        {
            //Arrange
            var eventAggregator = new EventAggregator();
            var viewModel = new AddBookingViewModel(
                eventAggregator, 
                _bookingManagerMock.Object, 
                _readOperationServiceMock.Object, 
                _createOperationServiceMock.Object, 
                _updateOperationServiceMock.Object, 
                _deleteOperationServiceMock.Object);

            //Act
            eventAggregator.GetEvent<DisplayBookingEvent>().Publish(booking);

            //Assert
            Assert.Equal(booking, viewModel.Booking);
        }

        //Booking property tests.

        /// <summary>
        /// Verifies that the <see cref="AddBookingViewModel.BookedStudent"/> property is updated with the expected student 
        /// information when the <see cref="AddBookingViewModel.Booking"/> property is set. Ensures that the view model has 
        /// the correct student data associated with the selected booking.
        /// </summary>
        [Fact]
        public void BookingProperty_ValueUpdated_UpdatesBookedStudentProperty()
        {
            //Arrange
            var viewModel = new AddBookingViewModel(
                _eventAggregatorMock.Object, 
                _bookingManagerMock.Object, 
                _readOperationServiceMock.Object, 
                _createOperationServiceMock.Object, 
                _updateOperationServiceMock.Object, 
                _deleteOperationServiceMock.Object);

            //Act
            viewModel.Booking = _testBooking;

            //Assert
            Assert.NotNull(viewModel.BookedStudent);
            Assert.Equal(_testStudent, viewModel.BookedStudent);
        }

        //AddBooking tests.

        /// <summary>
        /// Verifies that the expected error message is set when invalid booking data is entered and the <see 
        /// cref="AddBookingViewModel.AddBooking"/> method is called. Ensures that incomplete records are not added to the 
        /// database, and the user is informed of the problem.
        /// </summary>
        [Fact]
        public async Task AddBooking_InvalidBooking_ErrorMessageUpdated()
        {
            //Arrange
            _viewModel.Booking = _invalidBooking;

            //Act
            await _viewModel.AddBooking();

            //Assert
            Assert.Equal(NoBookingDataMessage, _viewModel.ErrorMessage);
        }

        //MemberData

        /// <summary>
        /// Provides member data for the <see cref="Constructor_NullEventAggregator_ThrowsArgumentNullException"/> test. Sets 
        /// one of the parameters to <see langword="null"/> to ensure that an <see cref="ArgumentNullException"/> is thrown.
        /// </summary>
        public static IEnumerable<object[]> ConstructorParametersMemberData()
        {
            yield return new object[] {
                null!,
                new Mock<IBookingManager>().Object,
                new Mock<IReadOperationService>().Object,
                new Mock<ICreateOperationService>().Object,
                new Mock<IUpdateOperationService>().Object, 
                new Mock<IDeleteOperationService>().Object };
            yield return new object[] {
                new Mock<IEventAggregator>().Object,
                null!,
                new Mock<IReadOperationService>().Object,
                new Mock<ICreateOperationService>().Object,
                new Mock<IUpdateOperationService>().Object,
                new Mock<IDeleteOperationService>().Object };
            yield return new object[] {
                new Mock<IEventAggregator>().Object,
                new Mock<IBookingManager>().Object,
                null!,
                new Mock<ICreateOperationService>().Object,
                new Mock<IUpdateOperationService>().Object,
                new Mock<IDeleteOperationService>().Object };
            yield return new object[] {
                new Mock<IEventAggregator>().Object,
                new Mock<IBookingManager>().Object,
                new Mock<IReadOperationService>().Object,
                null!,
                new Mock<IUpdateOperationService>().Object,
                new Mock<IDeleteOperationService>().Object };
            yield return new object[] {
                new Mock<IEventAggregator>().Object,
                new Mock<IBookingManager>().Object,
                new Mock<IReadOperationService>().Object,
                new Mock<ICreateOperationService>().Object,
                null!,
                new Mock<IDeleteOperationService>().Object };
            yield return new object[] {
                new Mock<IEventAggregator>().Object,
                new Mock<IBookingManager>().Object,
                new Mock<IReadOperationService>().Object,
                new Mock<ICreateOperationService>().Object,
                new Mock<IUpdateOperationService>().Object,
                null! };
        }

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
