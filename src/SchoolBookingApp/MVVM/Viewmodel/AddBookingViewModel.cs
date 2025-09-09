using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddBookingViewModel : ViewModelBase
    {
        //Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly IBookingManager _bookingManager;
        private readonly IReadOperationService _readOperationService;
        private Booking _booking;
        private Student? _bookedStudent;
        private List<Booking> _allBookings;
        private bool _isNewBooking;

        //Properties
        public Booking Booking
        {
            get => _booking;
            set => SetProperty(ref _booking, value);
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

        public AddBookingViewModel(
            IEventAggregator eventAggregator, IBookingManager bookingManager, IReadOperationService readOperationService)
        {
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _bookingManager = bookingManager
                ?? throw new ArgumentNullException(nameof(bookingManager));
            _readOperationService = readOperationService 
                ?? throw new ArgumentNullException(nameof(readOperationService));

            _booking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
            _bookedStudent = null;
            _allBookings = _bookingManager.ListBookings().GetAwaiter().GetResult();
            _isNewBooking = true;

            _eventAggregator.GetEvent<DisplayBookingEvent>().Subscribe(param =>
            {
                if (param is Booking booking)
                {
                    Booking = booking;
                    BookedStudent = _readOperationService.GetStudentData(Booking.StudentId).GetAwaiter().GetResult();
                    _isNewBooking = false;
                }
            });
        }
    }
}
