using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddBookingViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IBookingManager _bookingManager;
        private Booking _booking;
        private bool _isNewBooking;

        public Booking Booking
        {
            get => _booking;
            set => SetProperty(ref _booking, value);
        }

        public AddBookingViewModel(IEventAggregator eventAggregator, IBookingManager bookingManager)
        {
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _bookingManager = bookingManager
                ?? throw new ArgumentNullException(nameof(bookingManager));

            _booking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
            _isNewBooking = true;

            _eventAggregator.GetEvent<DisplayBookingEvent>().Subscribe(param =>
            {
                if (param is Booking booking)
                {
                    Booking = booking;
                    _isNewBooking = false;
                }
            });
        }
    }
}
