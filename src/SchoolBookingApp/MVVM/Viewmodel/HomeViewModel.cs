using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Commands;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.View;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IBookingManager _bookingManager;
        private readonly IReadOperationService _readOperationService;
        
        private readonly List<Booking> _bookings;
        private Booking? _selectedBooking;
        private readonly List<SearchResult> _students;
        private readonly int _parentsCount;

        //private ChangePageCommand<BookingView> _changePageCommand; -> change to the booking view

        public List<Booking> Bookings => _bookings;
        public Booking? SelectedBooking
        {
            get => _selectedBooking;
            set => SetProperty(ref _selectedBooking, value);
        }
        public List<SearchResult> Students => _students;
        public int ParentsCount => _parentsCount;
        public int BookingsCount => _bookings.Count;
        public int StudentsCount => _students.Count;
        public double StudentsBookedPercentage => StudentsCount == 0 ? 0
            : (double)BookingsCount / StudentsCount * 100;
        public double StudentsUnbookedPercentage => 100 - StudentsBookedPercentage;
        public Booking? NextMeeting => _bookings
            .OrderBy(booking => booking.BookingDate)
            .FirstOrDefault(booking => booking.BookingDate >= DateTime.Now);

        //Text for UI features
        public string DashboardTitleText => "Bookings Dashboard";
        public string TotalStudentsText => "Total students";
        public string TotalBookingsText => "Total bookings";
        public string TotalParentsText => "Total parents";
        public string SelectBookingButtonText => "View booking";
        public string ListViewColumnHeaderDateText => "Date";
        public string ListViewColumnHeaderTimeText => "Time";
        public string ListViewColumnHeaderFirstNameText => "First Name";
        public string ListViewColumnHeaderLastNameText => "Last Name";

        public HomeViewModel(
            IEventAggregator eventAggregator, 
            IBookingManager bookingManager, 
            IReadOperationService readOperationService)
        {
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _bookingManager = bookingManager
                ?? throw new ArgumentNullException(nameof(bookingManager));
            _readOperationService = readOperationService 
                ?? throw new ArgumentNullException(nameof(readOperationService));

            _bookings = _bookingManager.ListBookings().GetAwaiter().GetResult();
            _students = _readOperationService.GetStudentList().GetAwaiter().GetResult();
            _parentsCount = _readOperationService.GetParentList().GetAwaiter().GetResult().Count;

            _bookings.Add(new Booking(3, "John", "Smith", new DateTime(2025, 9, 17), new TimeSpan(14, 0, 0)));
        }

        private void OnSubmit(Booking booking)
        {
            _eventAggregator.GetEvent<NavigateToViewEvent>().Publish(typeof(HomeView));
            //need to pass booking information (student id?) to next view? StudentView? BookingView?
        }
    }
}
