using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Serilog;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Defines a contract for the <see cref="BookingManager"/> class. Ensures that the class includes methods for creating, 
    /// editing and retrieving bookings.
    /// </summary>
    public interface IBookingManager
    {
        Task<bool> CreateBooking(Booking bookingInformation);
        Task<bool> UpdateBooking(Booking bookingInformation);
        Task<bool> DeleteBooking(Booking bookingInformation);
        Task<List<Booking>> ListBookings();
        Task<Booking> RetrieveBookingInformation(Booking booking);
        Task<bool> ClearBookings();
    }   

    /// <summary>
    /// A class for handling booking information for parents' evenings. Enables the user to manage bookings for a parents' 
    /// evening with methods to handle creating, editing and retrieving booking information. It also includes methods for 
    /// deleting individual bookings as well as clearing all the bookings. Allows the user to keep track of booking 
    /// information, ensuring that there are no clashes.
    /// </summary>
    public class BookingManager
    {
        private readonly SqliteConnection _connection;

        //SQL query strings
        private const string InsertBookingQuery = @"
            INSERT INTO Bookings (StudentId, BookingDate, TimeSlot)
            VALUES (@id, @date, @time);";

        public BookingManager(SqliteConnection connection)
        {
            _connection = connection
                ?? throw new ArgumentNullException(nameof(connection));
            if (_connection.State != ConnectionState.Open)
                throw new InvalidOperationException("The provided connection must be open.");
        }

        //Methods

        /// <summary>
        /// Creates a new booking in the <c>Bookings</c> table using the provided <see cref="Booking"/> information. Used 
        /// to add a new booking to the database. Validates the provided information and checks for conflicts with existing 
        /// data before attempting to create the new record.
        /// </summary>
        /// <param name="booking">The <see cref="Booking"/> record containing the information to be inserted into the 
        /// <c>Bookings</c> table. Includes the <c>StudentId</c>, <c>BookingDate</c> and <c>TimeSlot</c>.</param>
        /// <returns><c>true</c> if the correct number of records are inserted. <c>false</c> if the record failed to 
        /// successfully insert.</returns>
        /// <exception cref="ArgumentException">Thrown if any of the <paramref name="booking"/> information is invalid 
        /// or if a booking already exists for the given student or the given date and time.</exception>
        public async Task<bool> CreateBooking(Booking booking)
        {
            //Validate the booking information. ArgumentException will be thrown if the data is invalid.
            if (await ValidateBookingInformaton(booking))
                Log.Information("Booking information validated successfully.");

            //Check that the booking does not conflict with any existing bookings, both by student it and by date and time.
            if (!await IsUniqueBooking(booking))
                throw new ArgumentException("The provided booking information conflicts with an existing booking.", nameof(booking));

            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = InsertBookingQuery;
                command.Parameters.AddWithValue("@id", booking.StudentId);
                command.Parameters.AddWithValue("@date", booking.DateString);
                command.Parameters.AddWithValue("@time", booking.TimeString);
                int rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected == 1; //Only one row should be affected.
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while creating a new booking: {Message}", ex.Message);
                return false;
            }
        }


        //Private helper methods

        /// <summary>
        /// Verifies that the provided <see cref="Booking"/> information is valid before attempting to create or update a 
        /// databse record. Checks that a valid student Id, date and time have been provided.
        /// </summary>
        /// <param name="bookingInformation">The <see cref="Booking"/> record containing the booking information.</param>
        /// <returns><c>True</c> if all the data is valid.</returns>
        /// <exception cref="ArgumentException">Thrown if any of the data is invalid.</exception>
        private async Task<bool> ValidateBookingInformaton(Booking bookingInformation)
        {
            //Check the student Id is valid
            if (bookingInformation.StudentId <= 0)
                throw new ArgumentException("A valid student Id must be provided.", nameof(bookingInformation));

            if (!await IsValidStudent(bookingInformation.StudentId))
                throw new ArgumentException("The provided student Id does not exist for exactly one student.", nameof(bookingInformation));

            //Check the date is valid
            if (bookingInformation.BookingDate == null)
                throw new ArgumentException("A valid booking date must be provided.", nameof(bookingInformation));

            //Check the time is valid
            if (bookingInformation.TimeSlot == null)
                throw new ArgumentException("A valid booking time must be provided.", nameof(bookingInformation));

            return true;
        }

        /// <summary>
        /// Verifies that a student with the given studentId exists in the <c>Students</c> table. Used to check that a 
        /// student Id provided when creating or updating a booking is valid before attempting to add or update a record 
        /// in the <c>Bookings</c> table.
        /// </summary>
        /// <param name="studentId">The id number for the student to be validated.</param>
        /// <returns><c>True</c> if exactly one student with the given id number is found. <c>False</c> if the data cannot 
        /// be validated or if an incorrect number of students are found matching the id number.</returns>
        private async Task<bool> IsValidStudent(int studentId)
        {
            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = @"SELECT COUNT(*) AS Total FROM Students WHERE Id = @id;";
                command.Parameters.AddWithValue("@id", studentId);
                var result = await command.ExecuteScalarAsync();

                var count = Convert.ToInt32(result);
                return count == 1;
            }
            catch
            {
                Log.Error("An error occurred while validating the student Id.");
                return false;
            }
        }

        /// <summary>
        /// Verifies that the provided booking information does not conflict with any existing bookings in the 
        /// <c>Bookings</c> table. Ensures that no two bookings are made for the same date and time, and that the there are 
        /// not multiple bookings for the same student.
        /// </summary>
        /// <param name="bookingInformation">The <see cref="Booking"/> record containing the student id, date and time.</param>
        /// <returns><c>True</c> if the StudentId and combination of BookingDate and TimeSlot fields are unique. 
        /// <c>False</c> if any matches are found.</returns>
        private async Task<bool> IsUniqueBooking(Booking bookingInformation)
        {
            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) AS Total 
                    FROM Bookings 
                    WHERE (BookingDate = @date AND TimeSlot = @time)
                    OR StudentId = @id;";
                command.Parameters.AddWithValue("@date", bookingInformation.DateString);
                command.Parameters.AddWithValue("@time", bookingInformation.TimeString);
                command.Parameters.AddWithValue("@id", bookingInformation.StudentId);
                var result = await command.ExecuteScalarAsync();
                var count = Convert.ToInt32(result);
                return count == 0;
            }
            catch
            {
                Log.Error("An error occurred while checking for existing bookings.");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a string value from a <see cref="SqliteDataReader"/> for a given column name, returning an empty 
        /// string if the value is null or empty.
        /// </summary>
        /// <param name="reader">The <see cref="SqliteDataReader"/> used to read the search data.</param>
        /// <param name="columnName">The field name for the column containing the desired data.</param>
        /// <returns>A string containing the desired data.</returns>
        private static string GetSafeString(SqliteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) ? reader.GetString(ordinal) : string.Empty;
        }

        /// <summary>
        /// Retrieves an integer value from a <see cref="SqliteDataReader"/> for a given column name, returning 0 if the 
        /// value is null or empty.
        /// </summary>
        /// <param name="reader">The <see cref="SqliteDataReader"/> used to read the search data.</param>
        /// <param name="columnName">The field name for the column containing the desired data.</param>
        /// <returns>The integer value of the desired data.</returns>
        private static int GetSafeInt(SqliteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) ? reader.GetInt32(ordinal) : 0;
        }
    }
}
