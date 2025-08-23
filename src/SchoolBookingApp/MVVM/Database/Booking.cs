using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Database
{
    public record Booking(int StudentId, int BookingDate, int BookingTime);
}
