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
        private const string BookingAddedMessage = "Booking added successfully.";
        private const string BookingUpdatedMessage = "Booking updated successfully.";
        private const string BookingFailedToUpdateMessage = "Failed to update booking. Please try again.";
        private const string BookingDeletedMessage = "Booking deleted successfully.";
        private const string BookingFailedToDeleteMessage = "Failed to delete booking. Please try again.";
        private const string BookingFailedToAddMessage = "Failed to add booking. Please try again.";

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
        private readonly Booking _defaultBooking;
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
            _defaultBooking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
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

        //AddBooking tests.

        /// <summary>
        /// Verifies that the expected update message is set when invalid booking data is entered and the <see 
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
            Assert.Equal(NoBookingDataMessage, _viewModel.UpdateMessage);
        }

        /// <summary>
        /// Verifies that the <see cref="IBookingManager.CreateBooking"/> method is called when valid booking data is 
        /// entered and the <see cref="AddBookingViewModel.AddBooking"/> method is called. Ensures that valid bookings are 
        /// entered into the database successfully using the <see cref="BookingManager"/>.
        /// </summary>
        [Fact]
        public async Task AddBooking_ValidBooking_CallsCreateOperationService()
        {
            //Arrange
            _viewModel.Booking = _testBooking;
            _viewModel.IsNewBooking = true;
            _viewModel.StudentId = 1;
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";
            _viewModel.DateTime = new DateTime(2025, 12, 25, 12, 0, 0);
            _bookingManagerMock.Setup(x => x.CreateBooking(_testBooking))
                .Returns(Task.FromResult(true));

            //Act
            await _viewModel.AddBooking();

            //Assert
            _bookingManagerMock.Verify(x => x.CreateBooking(It.IsAny<Booking>()), Times.Once);
        }

        /// <summary>
        /// Verifies that the expected error message is set when the <see cref="IBookingManager.CreateBooking"/> method 
        /// fails to add a valid booking from the <see cref="AddBookingViewModel.AddBooking"/> method.
        /// </summary>
        [Fact]
        public async Task AddBooking_CreateBookingFails_ErrorMessageDisplayed()
        {
            //Arrange
            _viewModel.Booking = _testBooking;
            _viewModel.IsNewBooking = true;
            _viewModel.StudentId = 1;
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";
            _viewModel.DateTime = new DateTime(2025, 12, 25, 12, 0, 0);
            _bookingManagerMock.Setup(x => x.CreateBooking(_testBooking))
                .Returns(Task.FromResult(false));

            //Act
            await _viewModel.AddBooking();

            //Assert
            Assert.Equal(BookingFailedToAddMessage, _viewModel.UpdateMessage);
        }

        //UpdateBooking tests.

        /// <summary>
        /// Verifies that the expected update message is set when invalid booking data is entered and the <see 
        /// cref="AddBookingViewModel.updateBooking"/> method is called. Ensures that database records are not updated with 
        /// incomplete data, and the user is informed of the problem.
        /// </summary>
        [Fact]
        public async Task UpdateBooking_InvalidBooking_ErrorMessageUpdated()
        {
            //Arrange
            _viewModel.Booking = _invalidBooking;

            //Act
            await _viewModel.UpdateBooking();

            //Assert
            Assert.Equal(NoBookingDataMessage, _viewModel.UpdateMessage);
        }

        /// <summary>
        /// Verifies that the <see cref="IBookingManager.UpdateBooking"/> method is called when valid booking data is 
        /// entered and the <see cref="AddBookingViewModel.UpdateBooking"/> method is called. Ensures that valid bookings are 
        /// entered into the database successfully using the <see cref="BookingManager"/>.
        /// </summary>
        [Fact]
        public async Task UpdateBooking_ValidBooking_CallsUpdateBooking()
        {
            //Arrange
            _viewModel.Booking = _testBooking;
            _viewModel.IsNewBooking = false;
            _viewModel.StudentId = 1;
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";
            _viewModel.DateTime = DateTime.MinValue; //Ensures that a different time is set.
            _bookingManagerMock.Setup(x => x.UpdateBooking(It.IsAny<Booking>()))
                .Returns(Task.FromResult(true));

            //Act
            await _viewModel.UpdateBooking();

            //Assert
            _bookingManagerMock.Verify(x => x.UpdateBooking(It.IsAny<Booking>()), Times.Once);
        }

        /// <summary>
        /// Verifies that the expected error message is set when the <see cref="IBookingManager.UpdateBooking"/> method 
        /// fails to update a valid booking from the <see cref="AddBookingViewModel.UpdateBooking"/> method.
        /// </summary>
        [Fact]
        public async Task UpdateBooking_UpdateBookingFails_ErrorMessageDisplayed()
        {
            //Arrange
            _viewModel.Booking = _testBooking;
            _viewModel.IsNewBooking = false;
            _viewModel.StudentId = 1;
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";
            _viewModel.DateTime = new DateTime(2025, 11, 11, 11, 10, 0); //Ensure a different time is set.
            _bookingManagerMock.Setup(x => x.UpdateBooking(It.IsAny<Booking>()))
                .Returns(Task.FromResult(false));

            //Act
            await _viewModel.UpdateBooking();

            //Assert
            Assert.Equal(BookingFailedToUpdateMessage, _viewModel.UpdateMessage);
        }

        //DeleteBooking tests.

        /// <summary>
        /// Verifies that the <see cref="IBookingManager.DeleteBooking"/> method is called once and that the expected 
        /// success message is displayed. Ensures that the record is successfully deleted and the user is updated.
        /// </summary>
        [Fact]
        public async Task DeleteBooking_DeletesSuccessfully_DeleteBookingCalledOnceSuccessMessageDisplayer()
        {
            //Arrange
            _viewModel.Booking = _testBooking;
            _bookingManagerMock.Setup(x => x.DeleteBooking(_testBooking.StudentId))
                .Returns(Task.FromResult(true));

            //Act
            await _viewModel.DeleteBooking();

            //Assert
            _bookingManagerMock.Verify(x => x.DeleteBooking(_testBooking.StudentId), Times.Once);
            Assert.Equal(BookingDeletedMessage, _viewModel.UpdateMessage);
        }

        /// <summary>
        /// Verifies that the expected failure message is displayed when the booking is not deleted successfully from the 
        /// database. Ensures that the user is notified of a deletion failure.
        /// </summary>
        [Fact]
        public async Task DeleteBooking_BookingFailsToDelete_FailureMessageDisplayed()
        {
            //Arrange
            _viewModel.Booking = _testBooking;
            _bookingManagerMock.Setup(x => x.DeleteBooking(_testBooking.StudentId))
                .Returns(Task.FromResult(false));

            //Act
            await _viewModel.DeleteBooking();

            //Assert
            _bookingManagerMock.Verify(x => x.DeleteBooking(_testBooking.StudentId), Times.Once);
            Assert.Equal(BookingFailedToDeleteMessage, _viewModel.UpdateMessage);
        }

        //ClearForm tests.

        /// <summary>
        /// Verifies that the <see cref="AddBookingViewModel.ClearForm"/> method clears the <see cref="AddBookingViewModel.
        /// UpdateMessage"/> and resets the <see cref="AddBookingViewModel.Booking"/> to an empty <see cref="Booking"/> 
        /// <see langword="struct"/>. Ensures that the user can clear the displayed booking without altering the database.
        /// </summary>
        [Fact]
        public void ClearForm_BookingDataDisplayed_NoUpdateMessageDisplayedAndCurrentBookingEmpty()
        {
            //Arrange
            _viewModel.Booking = _testBooking;

            //Act
            _viewModel.ClearForm();

            //Assert
            Assert.Equal(string.Empty, _viewModel.UpdateMessage);
            Assert.Equal(null, _viewModel.Booking);
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
