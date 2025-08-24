using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Used to store Booking information for passing information to and retrieving information from the <c>Bookings</c> 
    /// table in the database by the <see cref="BookingManager"/> <see langword="class"/>.
    /// </summary>
    /// <param name="StudentId">The id of the student to be discussed in the meeting.</param>
    /// <param name="FirstName">The first name of the student to be discussed in the meeting.</param>
    /// <param name="LastName">The last name of the student to be discussed in the meeting.</param>
    /// <param name="BookingDate">The date of the meeting.</param>
    /// <param name="TimeSlot">The start time of the meeting.</param>
    /// <remarks>May contain partial data when creating or updating a record in the database. An empty string or integer 
    /// values <= 0 indicates a value that does not need to be entered into the database. When reading from the database 
    /// all properties should be assigned a value to ensure all necessary information for one or more bookings is made 
    /// available to the user. A valid date and time must always be entered.</remarks>
    public readonly record struct Booking(
        int StudentId, string FirstName, string LastName, DateTime? BookingDate, TimeSpan? TimeSlot)
    {
        /// <summary>
        /// Alternative constructor for inputting a new booking to the Bookings table with the date and time as a single 
        /// DateTime object.
        /// </summary>
        /// <param name="studentId">The id number for the selected student.</param>
        /// <param name="dateTime">The booking date and start time for the meeting as a single DateTime object.</param>
        public Booking(int studentId, DateTime? dateTime)
            : this(studentId, string.Empty, string.Empty, dateTime?.Date, dateTime?.TimeOfDay) { }

        /// <summary>
        /// Alternative constructor for inputting a new booking to the Bookings table with the date and time as strings. 
        /// Used when reading from the database where date and time are stored as TEXT.
        /// </summary>
        /// <param name="studentId">The id for the selected student.</param>
        /// <param name="date">The date of the meeting as a string in the format "yyyy-MM-dd".</param>
        /// <param name="time">The start time of the meeting as a string in the format "HH:mm".</param>
        public Booking(int studentId, string date, string time)
            : this(studentId, string.Empty, string.Empty,
                  DateTime.TryParse(date, out var bookingDate) ? bookingDate.Date : null,
                  TimeSpan.TryParse(time, out var timeSlot) ? timeSlot : null) { }


        /// <summary>
        /// Retrieves the booking date as a string in the format "yyyy-MM-dd". Used for storing the date in the database 
        /// as TEXT.
        /// </summary>
        public string? DateString => BookingDate?.ToString("yyyy-MM-dd");

        /// <summary>
        /// Retrieves the booking time as a string in the format "HH:mm". Used for storing the time in the database 
        /// as TEXT.
        /// </summary>
        public string? TimeString => TimeSlot?.ToString(@"hh\:mm");
    }
}
