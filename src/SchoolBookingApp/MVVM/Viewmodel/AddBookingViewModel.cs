using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddBookingViewModel : ViewModelBase
    {
        //Constant error messages
        private const string NoBookingDataMessage = "Complete all fields before adding booking.";
        private const string BookingAddedMessage = "Booking added successfully.";
        private const string BookingFailedToAddMessage = "Failed to add booking. Please try again.";

        //UI labels
        public string AddUpdateButtonLabel => _isNewBooking ? "Add Booking" : "Update Booking";

        //Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly IBookingManager _bookingManager;
        private readonly IReadOperationService _readOperationService;
        private readonly ICreateOperationService _createOperationService;
        private readonly IUpdateOperationService _updateOperationService;
        private readonly IDeleteOperationService _deleteOperationService;
        private Booking _booking;
        private Student? _bookedStudent;
        private List<Booking> _allBookings;
        private bool _isNewBooking;
        private string _updateMessage;

        private ICommand? _addBookingCommand;
        private ICommand? _updateBookingCommand;
        private ICommand? _deleteBookingCommand;
        private ICommand? _showDataCommand;
        private ICommand? _showCommentsCommand;

        //Properties
        public Booking Booking
        {
            get => _booking;
            set
            {
                SetProperty(ref _booking, value);
                BookedStudent = _booking.StudentId > 0 ? 
                    _readOperationService
                    .GetStudentData(_booking.StudentId)
                    .GetAwaiter()
                    .GetResult() 
                    : null;
            }
        }
        public Student? BookedStudent
        {
            get => _bookedStudent;
            set => SetProperty(ref _bookedStudent, value);
        }
        public List<Booking> AllBookings
        {
            get => _allBookings;
            set => SetProperty(ref _allBookings, value);
        }
        public string UpdateMessage
        {
            get => _updateMessage;
            set => SetProperty(ref _updateMessage, value);
        }

        //Commands

        public AddBookingViewModel(
            IEventAggregator eventAggregator,
            IBookingManager bookingManager,
            IReadOperationService readOperationService, 
            ICreateOperationService createOperationService, 
            IUpdateOperationService updateOperationService, 
            IDeleteOperationService deleteOperationService)
        {
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _bookingManager = bookingManager
                ?? throw new ArgumentNullException(nameof(bookingManager));
            _readOperationService = readOperationService 
                ?? throw new ArgumentNullException(nameof(readOperationService));
            _createOperationService = createOperationService 
                ?? throw new ArgumentNullException(nameof(createOperationService));
            _updateOperationService = updateOperationService 
                ??  throw new ArgumentNullException(nameof(updateOperationService));
            _deleteOperationService = deleteOperationService 
                ?? throw new ArgumentNullException(nameof(deleteOperationService));

            _booking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
            _bookedStudent = null;
            _allBookings = _bookingManager.ListBookings().GetAwaiter().GetResult();
            _isNewBooking = true;
            _updateMessage = string.Empty;

            _eventAggregator.GetEvent<DisplayBookingEvent>().Subscribe(param =>
            {
                if (param is Booking booking)
                {
                    Booking = booking;
                    _isNewBooking = false;
                }
            });
        }

        //Methods

        public async Task AddBooking()
        {
            UpdateMessage = string.Empty;

            if (!IsValidBooking())
            {
                UpdateMessage = NoBookingDataMessage;
                return;
            }

            bool bookingAdded = await _bookingManager.CreateBooking(_booking);

            if (bookingAdded)
            {
                UpdateMessage = BookingAddedMessage;
                AllBookings = await _bookingManager.ListBookings();
                ResetBooking();
            }
            else 
            {                 
                UpdateMessage = BookingFailedToAddMessage;
            }
        }

        public async Task UpdateBooking()
        {
            UpdateMessage = string.Empty;
        }

        public async Task DeleteBooking()
        {
            UpdateMessage = string.Empty;


        }

        //Helper methods

        /// <summary>
        /// Checks that all necessary fields in the <see cref="_booking"/> record contain valid data. Used to ensure that 
        /// a booking can be added to the database.
        /// </summary>
        /// <returns><c>true</c> if all the necessary data has been set for the <see cref="Booking"/> instance. <c>false</c> 
        /// if any data is missing or invalid.</returns>
        private bool IsValidBooking()
        {
            return _booking.StudentId > 0 &&
                   !string.IsNullOrWhiteSpace(_booking.FirstName) &&
                   !string.IsNullOrWhiteSpace(_booking.LastName) &&
                   !string.IsNullOrWhiteSpace(_booking.DateString) &&
                   !string.IsNullOrWhiteSpace(_booking.TimeString);
        }

        private bool IsSqlInjectionSafe()
        {
            return _booking.FirstName.All(c => char.IsLetter(c) || c == '-' || c == ' ') &&
                   _booking.LastName.All(c => char.IsLetter(c) || c == '-' || c == ' ');
        }

        /// <summary>
        /// Resets the <see cref="_booking"/> field to a new instance of the <see cref="Booking"/> record with default data 
        /// ready to add a new booking or view an existing booking. Sets the <see cref="_isNewBooking"/> flag to <c>true</c> 
        /// to indicate that the next data added will be for a new booking.
        /// </summary>
        private void ResetBooking()
        {
            Booking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
            _isNewBooking = true;
        }
    }
}
