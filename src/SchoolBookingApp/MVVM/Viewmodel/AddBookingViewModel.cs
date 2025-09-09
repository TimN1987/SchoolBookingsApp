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
        private string _errorMessage;

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
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
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
            _errorMessage = string.Empty;

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
            ErrorMessage = string.Empty;

            if (!IsValidBooking())
            {
                ErrorMessage = NoBookingDataMessage;
                return;
            }
        }

        public async Task UpdateBooking()
        {
            ErrorMessage = string.Empty;
        }

        public async Task DeleteBooking()
        {
            ErrorMessage = string.Empty;


        }

        //Helper methods
        private bool IsValidBooking()
        {
            return _booking.StudentId > 0 &&
                   !string.IsNullOrWhiteSpace(_booking.FirstName) &&
                   !string.IsNullOrWhiteSpace(_booking.LastName) &&
                   !string.IsNullOrWhiteSpace(_booking.DateString) &&
                   !string.IsNullOrWhiteSpace(_booking.TimeString);
        }   

        private void ResetBooking()
        {
            _booking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
            OnPropertyChanged(nameof(Booking));
            _isNewBooking = true;
        }
    }
}
