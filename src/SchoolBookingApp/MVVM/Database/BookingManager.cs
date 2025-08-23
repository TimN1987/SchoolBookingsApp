using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SchoolBookingApp.MVVM.Database
{
    public interface IBookingManager
    {
        Task<bool> CreateBooking(Booking bookingInformation);
        Task<bool> UpdateBooking(Booking bookingInformation);
        Task<bool> DeleteBooking(Booking bookingInformation);
        Task<List<Booking>> ListBookings();
        Task<Booking> RetrieveBookingInformation(Booking booking);
    }   

    public class BookingManager
    {
        private readonly SqliteConnection _connection;

        public BookingManager(SqliteConnection connection)
        {
            _connection = connection
                ?? throw new ArgumentNullException("Sqlite connection cannot be null.");
            if (_connection.State != ConnectionState.Open)
                throw new InvalidOperationException("The provided connection must be open.");
        }
    }
}
