using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32.SafeHandles;
using SchoolBookingApp.MVVM.Commands;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.Viewmodel.Base;
using Serilog;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddBookingViewModel : ViewModelBase
    {
        //Constant error messages
        private const string NoBookingDataMessage = "Complete all fields before adding booking.";
        private const string BookingAddedMessage = "Booking added successfully.";
        private const string BookingFailedToAddMessage = "Failed to add booking. Please try again.";
        private const string BookingUpdatedMessage = "Booking updated successfully.";
        private const string BookingFailedToUpdateMessage = "Failed to update booking. Please try again.";
        private const string BookingDeletedMessage = "Booking deleted successfully.";
        private const string BookingFailedToDeleteMessage = "Failed to delete booking. Please try again.";
        private const string SqlInjectionMessage = "Invalid characters in name fields.";

        //UI labels
        public string AddUpdateButtonLabel => _isNewBooking ? "Add Booking" : "Update Booking";
        public string DeleteBookingButtonLabel => "Delete Booking";
        public string ClearFormButtonLabel => "Clear Form";
        public string ShowHideDataButtonLabel => IsBookingDataVisible ? "Hide Data" : "Show Data";
        public string ShowHideCommentsButtonLabel => IsCommentsVisible ? "Hide Comments" : "Show Comments";

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
        private bool _isBookingDataVisible;
        private bool _isCommentsVisible;

        private ICommand? _addBookingCommand;
        private ICommand? _updateBookingCommand;
        private ICommand? _deleteBookingCommand;
        private ICommand? _loadBookingCommand;
        private ICommand? _toggleBookingDataVisibilityCommand;
        private ICommand? _toggleCommentsVisibilityCommand;

        //Properties
        public Booking Booking
        {
            get => _booking;
            set
            {
                if (!IsSqlInjectionSafe(value))
                {
                    UpdateMessage = SqlInjectionMessage;
                    return;
                }
                
                UpdateMessage = string.Empty;
                SetProperty(ref _booking, value);
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
        public bool IsBookingDataVisible
        {
            get => _isBookingDataVisible;
            set
            {
                SetProperty(ref _isBookingDataVisible, value);
                OnPropertyChanged(nameof(ShowHideDataButtonLabel));
            }
        }
        public bool IsCommentsVisible
        {
            get => _isCommentsVisible;
            set
            {
                SetProperty(ref _isCommentsVisible, value);
                OnPropertyChanged(nameof(ShowHideCommentsButtonLabel));
            }
        }

        //Commands

        public ICommand? AddBookingCommand => _addBookingCommand 
            ?? new RelayCommand(async param => await AddBooking());
        public ICommand? UpdateBookingCommand => _updateBookingCommand
            ?? new RelayCommand(async param => await UpdateBooking());
        public ICommand? DeleteBookingCommand => _deleteBookingCommand
            ?? new RelayCommand(async param => await DeleteBooking());
        public ICommand? LoadBookingCommand => _loadBookingCommand
            ?? null;
        public ICommand? ToggleBookingDataVisibilityCommand => _toggleBookingDataVisibilityCommand
            ?? new RelayCommand(param => IsBookingDataVisible = !IsBookingDataVisible);
        public ICommand? ToggleCommentsVisibilityCommand => _toggleCommentsVisibilityCommand
            ?? new RelayCommand(param => _isCommentsVisible = !_isCommentsVisible);

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
            _isBookingDataVisible = false;
            _isCommentsVisible = false;

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

        /// <summary>
        /// Adds a new booking to the database using the data stored in the <see cref="_booking"/> field. Validates that 
        /// all necessary data has been entered before attempting to add the booking. If the booking is added successfully, 
        /// the <see cref="AllBookings"/> property is updated to include the new booking and the <see cref="_booking"/> 
        /// field is reset to a new instance of the <see cref="Booking"/> record with default data ready to add another 
        /// booking. A success or failure message is stored in the <see cref="UpdateMessage"/> property to inform the user 
        /// of the outcome.
        /// </summary>
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
                AllBookings = await _bookingManager.ListBookings(); //Update bookings list with new booking.
                ResetBooking();
                UpdateMessage = BookingAddedMessage;
            }
            else 
            {                 
                UpdateMessage = BookingFailedToAddMessage;
            }
        }

        /// <summary>
        /// Updates an existing booking in the database using the data stored in the <see cref="_booking"/> field. 
        /// Validates that all necessary data has been entered before attempting to update the booking. Updates the 
        /// <see cref="AllBookings"/> property to include the updated booking data if the update is successful and reserts 
        /// the form to a new instance of the <see cref="Booking"/> record with default data ready to add a new booking. 
        /// The <see cref="UpdateMessage"/> property is updated with a success or failure message to inform the user of 
        /// the outcome.
        /// </summary>
        public async Task UpdateBooking()
        {
            UpdateMessage = string.Empty;

            if (!IsValidBooking())
            {
                UpdateMessage = NoBookingDataMessage;
                return;
            }

            bool bookingUpdated = await _bookingManager.UpdateBooking(_booking);

            if (bookingUpdated)
            {
                AllBookings = await _bookingManager.ListBookings(); //Update bookings list with updated booking data.
                ResetBooking();
                UpdateMessage = BookingUpdatedMessage;
            }
            else
            {
                UpdateMessage = BookingFailedToUpdateMessage;
            }
        }

        /// <summary>
        /// Deletes the selected booking from the database. Displays an <see cref="UpdateMessage"/> to inform the user of 
        /// the outcome of the deletion.
        /// </summary>
        public async Task DeleteBooking()
        {
            bool bookingDeleted = await _bookingManager.DeleteBooking(_booking.StudentId);
            UpdateMessage = bookingDeleted ? BookingDeletedMessage : BookingFailedToDeleteMessage;
        }

        /// <summary>
        /// Clears all information from the form to start a new booking without affecting the current booking.
        /// </summary>
        public void ClearForm()
        {
            UpdateMessage = string.Empty;
            ResetBooking();
        }

        //Helper methods

        /// <summary>
        /// Updates the <see cref="BookedStudent"/> property when a <see cref="Booking"/> property is loaded to ensure 
        /// that the correct student data is available for display in the view.
        /// </summary>
        private async Task OnBookingLoaded()
        {
            BookedStudent = _booking.StudentId > 0 ?
                await _readOperationService.GetStudentData(_booking.StudentId) : 
                null;
        }

        /// <summary>
        /// Checks that all necessary fields in the <see cref="_booking"/> record contain valid data. Used to ensure that 
        /// a booking can be added to the database.
        /// </summary>
        /// <returns><c>true</c> if all the necessary data has been set for the <see cref="Booking"/> instance. <c>false</c> 
        /// if any data is missing or invalid.</returns>
        private bool IsValidBooking()
        {
            if (!IsValidDate() || !IsValidTime())
                return false;
            
            return _booking.StudentId > 0 &&
                   !string.IsNullOrWhiteSpace(_booking.FirstName) &&
                   !string.IsNullOrWhiteSpace(_booking.LastName) &&
                   !string.IsNullOrWhiteSpace(_booking.DateString) &&
                   !string.IsNullOrWhiteSpace(_booking.TimeString);
        }

        /// <summary>
        /// Checks that the first name and last name fields in the <see cref="_booking"/> record do not contain any 
        /// characters that could be used in a SQL injection attack. Only letters, hyphens, spaces and apostrophes are 
        /// allowed.
        /// </summary>
        /// <param name="booking">The <see cref="Booking"/> record to be checked for invalid characters.</param>
        /// <returns><see langword="true"/> if all the characters stored in the name properties of the <see cref="Booking"/> 
        /// are on the whitelist. <see langword="false"/> if any invalid characters are entered.</returns>
        private bool IsSqlInjectionSafe(Booking booking)
        {
            return booking.FirstName.All(c => char.IsLetter(c) || c == '-' || c == ' ' || c == '\'') &&
                   booking.LastName.All(c => char.IsLetter(c) || c == '-' || c == ' ' || c == '\'');
        }

        /// <summary>
        /// Verifies that the date string in the <see cref="_booking"/> record is in the correct format of "yyyy-Mm-dd". 
        /// Ensures that the date can be parsed correctly when adding a booking to the database.
        /// </summary>
        /// <returns><see langword="true"/> if a valid date is stored in the <see cref="Booking"/>. <see langword="false"/> 
        /// if the date has not been stored in the correct format.</returns>
        private bool IsValidDate()
        {
            return DateTime.TryParseExact(
                _booking.DateString, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        /// <summary>
        /// Verifies that the time string in the <see cref="_booking"/> record is in the correct format of "hh:mm". Ensures 
        /// that the time can be parsed correctly when adding a booking to the database.
        /// </summary>
        /// <returns><see langword="true"/> if a valid time is stored in the <see cref="Booking"/>. <see langword="false"/> 
        /// if the time has not been stored in the correct format.</returns>
        private bool IsValidTime()
        {
            return TimeSpan.TryParseExact(
                _booking.TimeString, 
                "hh\\:mm", 
                CultureInfo.InvariantCulture, 
                out _);
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
