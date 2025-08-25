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
        Task<bool> DeleteBooking(int studentId);
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
        private const string UpdateBookingQuery = @"
            UPDATE Bookings
            SET BookingDate = @date, TimeSlot = @time
            WHERE StudentId = @id;";
        private const string DeleteBookingQuery = @"
            DELETE FROM Bookings";
        private const string BookingInformationQuery = @"
            SELECT b.StudentId, s.FirstName, s.LastName, b.BookingDate, b.TimeSlot 
            FROM Bookings AS b 
            LEFT JOIN Students AS s 
            ON b.StudentId = s.Id";
        private const string StudentIdCondition = @" WHERE StudentId = @id;";

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
            if (await ValidateBookingInformation(booking))
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

        /// <summary>
        /// Updates a specified booking in the <c>Bookings</c> table using the provided <see cref="Booking"/> information. 
        /// Used to modify an existing booking in the database. Validates the provided information, checking that the 
        /// selected student already has a booking, and checks for conflicts with existing data before attempting to update 
        /// the record.
        /// </summary>
        /// <param name="bookingInformationToUpdate">A <see cref="Booking"/> <see langword="struct"/> containing a 
        /// valid student id where the student already has a booking in the <c>Bookings</c> table, and a booking date and 
        /// time which have not already been booked.</param>
        /// <returns><c>True</c> if the record is successfully updated. <c>False if the record fails to update correctly.</c></returns>
        /// <exception cref="ArgumentException">Thrown if the student id, date or time are invalid, or if the given 
        /// student does not have a booking already or the booking data and time are already booked.</exception>
        public async Task<bool> UpdateBooking(Booking bookingInformationToUpdate)
        {
            //Validate the booking information. ArgumentException will be thrown if the data is invalid.
            if (await ValidateBookingInformation(bookingInformationToUpdate))
                Log.Information("Booking update information validated successfully.");

            /* Check that the updated booking date and time does not conflict with any existing bookings and that there is 
            already a booking for the given student. */
            if (!await IsUniqueBooking(bookingInformationToUpdate, isUpdate: true))
                throw new ArgumentException("The provided booking information conflicts with an existing booking.", nameof(bookingInformationToUpdate));

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = UpdateBookingQuery;
                command.Parameters.AddWithValue("@id", bookingInformationToUpdate.StudentId);
                command.Parameters.AddWithValue("@date", bookingInformationToUpdate.DateString);
                command.Parameters.AddWithValue("@time", bookingInformationToUpdate.TimeString);
                int rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected == 1; //Only one row should be affected during the update process.
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while updating the booking: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes a record from the <c>Bookings</c> table where the StudentId field matches the given <paramref 
        /// name="studentId"/>. Checks that the given <paramref name="studentId"/> is valid, and that a booking already 
        /// exists in the <c>Bookings</c> table to be deleted. Used when a single booking needs to be removed from the 
        /// <c>Bookings</c> table rather than changing the date or timeslot.
        /// </summary>
        /// <param name="studentId">The StudentId field for the record to be deleted.</param>
        /// <returns><c>True</c> if the record is successfully deleted. <c>False</c> if the record could not be deleted.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="studentId"/> is invalid or if there is not a 
        /// record with the given <paramref name="studentId"/> to delete.</exception>
        public async Task<bool> DeleteBooking(int studentId)
        {
            if (!await IsValidStudent(studentId))
                throw new ArgumentException("Cannot delete a record with an invalid student id. The id must be > 0 and a booking must already exist for the student id.");

            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = DeleteBookingQuery + StudentIdCondition;
                command.Parameters.AddWithValue("@id", studentId);
                int rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected == 1;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while deleting a record: {Message}.", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a list of <see cref="Booking"/> <see langword="struct"/>s containing all the bookings from the 
        /// <c>Bookings</c> table with the matching names from the <c>Students</c> table. Includes the students ids with 
        /// the matching first names, last names, booking dates and time slots. Used to display all bookings for a user to 
        /// see a timetable or visually identify gaps in the schedule.
        /// </summary>
        /// <returns>A list of <see cref="Booking"/> <see langword="struct"/>s containing all the bookings in the 
        /// database.</returns>
        public async Task<List<Booking>> ListBookings()
        {
            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = BookingInformationQuery + ';';
                await using var reader = await command.ExecuteReaderAsync();

                var bookingsList = new List<Booking>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Booking booking = GetSafeBooking(reader);
                        bookingsList.Add(booking);
                    }
                }

                return bookingsList;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while retrieving the bookings list: {Message}.", ex.Message);
                throw;
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
        private async Task<bool> ValidateBookingInformation(Booking bookingInformation)
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
            if (studentId <= 0)
                return false;
            
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
        private async Task<bool> IsUniqueBooking(Booking bookingInformation, bool isUpdate = false)
        {
            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT COUNT(*) AS Total 
                    FROM Bookings 
                    WHERE BookingDate = @date AND TimeSlot = @time;";
                command.Parameters.AddWithValue("@date", bookingInformation.DateString);
                command.Parameters.AddWithValue("@time", bookingInformation.TimeString);
                var result = await command.ExecuteScalarAsync();
                var count = Convert.ToInt32(result);

                command.CommandText = @"
                        SELECT COUNT(*) AS Total 
                        FROM Bookings 
                        WHERE StudentId = @id;";
                command.Parameters.AddWithValue("@id", bookingInformation.StudentId);
                result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);

                /*If this is an update operation, the student Id should already have a booking and so the final count 
                value should be 1. For a create operation, there should be no existing bookings and no matching date and 
                time pairings, so the final count should be 0.*/

                return isUpdate ? count == 1 : count == 0;
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

        /// <summary>
        /// Safely retrieves all booking information from a <see cref="SqliteDataReader"/> and returns it as a <see 
        /// cref="Booking"/> <see langword="struct"/>. Used to safely retrieve all data for a given booking from the 
        /// <c>Bookings</c> and <c>Students</c> tables.
        /// </summary>
        /// <param name="reader">A <see cref="SqliteDataReader"/> used to read the booking information from the 
        /// database.</param>
        private static Booking GetSafeBooking(SqliteDataReader reader)
        {
            var bookingInformation = new Booking
            (
                GetSafeInt(reader, "StudentId"),
                GetSafeString(reader, "FirstName"),
                GetSafeString(reader, "LastName"),
                GetSafeString(reader, "BookingDate"),
                GetSafeString(reader, "TimeSlot")
            );

            return bookingInformation;

        }
    }
}
