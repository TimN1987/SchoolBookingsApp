using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Database;
using Serilog;

namespace SchoolBookingAppTests.DatabaseTests
{
    public class BookingManagerTests
    {
        private const string TestConnectionString = "Data Source=:memory:";
        private const int TotalRecordsAfterOneInsertion = 6;

        private readonly DatabaseConnectionInformation _connectionInformation;
        private readonly SqliteConnection _connection;
        private readonly SqliteConnection _closedConnection;
        private readonly DatabaseInitializer _databaseInitializer;
        private readonly BookingManager _bookingManager;

        public BookingManagerTests()
        {
            _connectionInformation = new DatabaseConnectionInformation();

            _connection = new SqliteConnection(TestConnectionString);
            _connection.Open();

            _closedConnection = new SqliteConnection(TestConnectionString);

            _databaseInitializer = new DatabaseInitializer(_connection, _connectionInformation);
            _databaseInitializer.InitializeDatabaseAsync().GetAwaiter().GetResult();

            _bookingManager = new BookingManager(_connection);
        }


        /// <summary>
        /// Checks that an <see cref="ArgumentNullException"/> is thrown if a null parameter is passed to the constructor. 
        /// Ensures that the <see cref="SqliteConnection"/> is not <see langword="null"/> which would lead to errors in 
        /// accessing the database.
        /// </summary>
        [Fact]
        public void Constructor_NullInput_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BookingManager(null!));
        }

        /// <summary>
        /// Verifies that an instance of <see cref="BookingManager"/> is successfully created  when a valid
        /// connection is provided to its constructor. Ensures that no exceptions are thrown and that the created instance 
        /// is not null and is of the correct type.
        /// </summary>
        [Fact]
        public void Constructor_ValidConnection_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var bookingManager = new BookingManager(_connection);

            //Assert
            Assert.NotNull(bookingManager);
            Assert.IsType<BookingManager>(bookingManager);
        }

        /// <summary>
        /// Verifies that an <see cref="InvalidOperationException"/> is thrown if a closed connection is passed as the 
        /// parameter of the Constructor. Ensures that the <see cref="BookingManager"/> is not relying on a 
        /// closed connection to read from the database and that the connection is handled separately.
        /// </summary>
        [Fact]
        public void Constructor_ClosedConnection_ThrowsInvalidOperationException()
        {
            //Arrange, Act & Assert
            Assert.Throws<InvalidOperationException>(() => new BookingManager(_closedConnection));
        }

        //CreateBooking tests.

        /// <summary>
        /// Verifies that an <see cref="ArgumentException"/> is thrown when invalid booking information is provided to the 
        /// <see cref="BookingManager.CreateBooking(Booking)"/> method. Uses member data to provide a range of invalid 
        /// data with cases where one or more elements should throw an <see cref="ArgumentException"/>. Ensures that any 
        /// invalid data is correctly identified and handled, preventing invalid records from being created in the database 
        /// or displayed to the user.
        /// </summary>
        /// <param name="invalidBookingInformation">A <see cref="Booking"/> record containing invalid data.</param>
        [Theory]
        [MemberData(nameof(CreateBookingInvalidMemberData))]
        public async Task CreateBooking_InvalidBookingInformation_ThrowsArgumentException(Booking invalidBookingInformation)
        {
            //Arrange
            await ClearTables();
            await AddDefaultData();

            //Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _bookingManager.CreateBooking(invalidBookingInformation));
        }

        [Theory]
        [MemberData(nameof(CreateBookingValidMemberData))]
        public async Task CreateBooking_ValidBookingInformation_CreatesBookingSuccessfully(Booking validBookingInformation)
        {
            //Arrange
            await ClearTables();
            await AddDefaultData();

            //Act
            var result = await _bookingManager.CreateBooking(validBookingInformation);

            //Assert
            Assert.True(result);
            Assert.True(
                await CheckEntryDetails(
                    validBookingInformation.StudentId,
                    validBookingInformation.DateString ?? string.Empty,
                    validBookingInformation.TimeString ?? string.Empty
                    ));
            Assert.Equal(TotalRecordsAfterOneInsertion, await CountBookings());
        }



        //Member Data

        /// <summary>
        /// Provides member data for the <see cref="CreateBooking_InvalidBookingInformation_ThrowsArgumentException"/> test. 
        /// Includes a range of invalid data with cases where one or more elements should throw an <see 
        /// cref="ArgumentException"/>. This includes values <= 0, dates and times in an invalid format and a student Id 
        /// that is not found in the <c>Students</c> table.
        /// </summary>
        public static IEnumerable<object[]> CreateBookingInvalidMemberData()
        {
            yield return new object[] { new Booking(-1, string.Empty, string.Empty, new DateTime(2025, 09, 14), new TimeSpan(16, 0, 0)) };
            yield return new object[] { new Booking(11, string.Empty, string.Empty, new DateTime(2025, 09, 14), new TimeSpan(16, 0, 0)) };
            yield return new object[] { new Booking(1, string.Empty, string.Empty, null, new TimeSpan(16, 0, 0)) };
            yield return new object[] { new Booking(1, string.Empty, string.Empty, new DateTime(2025, 09, 14), null) };
            yield return new object[] { new Booking(-5, string.Empty, string.Empty, null, null) };
            yield return new object[] { new Booking(20, string.Empty, string.Empty, new DateTime(2025, 09, 14), null) };
            yield return new object[] { new Booking(7, string.Empty, string.Empty, null, new TimeSpan(16, 0, 0)) };
            yield return new object[] { new Booking(0, "2025-09-14", "16:00") };
            yield return new object[] { new Booking(3, "202-13-32", "16:00") };
            yield return new object[] { new Booking(3, "2025-09-14", "25:61") };
            yield return new object[] { new Booking(3, "invalid-date", "invalid-time") };
            yield return new object[] { new Booking(3, "", "") };
            yield return new object[] { new Booking(-1, new DateTime(2025, 09, 14, 16, 0, 0)) };
            yield return new object[] { new Booking(15, new DateTime(2025, 09, 14, 16, 0, 0)) };
            yield return new object[] { new Booking(4, null) };
        }

        /// <summary>
        /// Provides member data for the <see cref="CreateBooking_ValidBookingInformation_CreatesBookingSuccessfully"/> 
        /// test. Includes a range of valid data with different student Ids, dates and times using the different 
        /// constructors of the <see cref="Booking"/> <see langword="struct"/>.
        /// </summary>
        public static IEnumerable<object[]> CreateBookingValidMemberData()
        {
            yield return new object[] { new Booking(6, string.Empty, string.Empty, new DateTime(2025, 09, 14), new TimeSpan(16, 0, 0)) };
            yield return new object[] { new Booking(7, string.Empty, string.Empty, new DateTime(2025, 10, 01), new TimeSpan(17, 30, 0)) };
            yield return new object[] { new Booking(8, string.Empty, string.Empty, new DateTime(2025, 11, 20), new TimeSpan(15, 45, 0)) };
            yield return new object[] { new Booking(9, string.Empty, string.Empty, new DateTime(2025, 12, 05), new TimeSpan(18, 15, 0)) };
            yield return new object[] { new Booking(10, string.Empty, string.Empty, new DateTime(2026, 01, 10), new TimeSpan(16, 30, 0)) };
            yield return new object[] { new Booking(6, "2025-09-14", "16:00") };
            yield return new object[] { new Booking(7, "2025-10-01", "17:30") };
            yield return new object[] { new Booking(8, "2025-11-20", "15:45") };
            yield return new object[] { new Booking(9, "2025-12-05", "18:15") };
            yield return new object[] { new Booking(10, "2026-01-10", "16:30") };
            yield return new object[] { new Booking(6, new DateTime(2025, 09, 14, 16, 0, 0)) };
            yield return new object[] { new Booking(7, new DateTime(2025, 10, 01, 17, 30, 0)) };
            yield return new object[] { new Booking(8, new DateTime(2025, 11, 20, 15, 45, 0)) };
            yield return new object[] { new Booking(9, new DateTime(2025, 12, 05, 18, 15, 0)) };
            yield return new object[] { new Booking(10, new DateTime(2025, 01, 11, 15, 00, 0)) };
        }

        //Helper Methods

        /// <summary>
        /// Used to ensure that no unexpected data is left in the <c>Bookings</c> or <c>Students</c> table before starting 
        /// a test, so that tests are run using the correct starting data. This is important to ensure that tests are not 
        /// affected by unexpected database entries and that results are consistent.
        /// </summary>
        public async Task<bool> ClearTables()
        {
            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = @"
                    DELETE FROM Bookings;
                    DELETE FROM Students;";
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected >= 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while clearing the Bookings table.");
                throw;
            }
        }

        /// <summary>
        /// Adds the starting test data to the <c>Bookings</c> and <c>Students</c> tables to ensure consistent start 
        /// conditions for tests. Ensures that the outcomes of the test can be correctly predicted and verified.
        /// </summary>
        public async Task<bool> AddDefaultData()
        {
            try
            {
                using var transaction = _connection.BeginTransaction();

                await using var studentsCommand = _connection.CreateCommand();
                studentsCommand.CommandText = @"
                    INSERT INTO Students 
                        (FirstName, LastName, DateOfBirth, Class) 
                    VALUES
                        ('John', 'Doe', 20190412, '5B'),
                        ('Jane', 'Smith', 20190323, '5A'),
                        ('Alice', 'Johnson', 20190530, '5C'),
                        ('Bob', 'Brown', 20190214, '5B'),
                        ('Charlie', 'Davis', 20190101, '5A'),
                        ('Ahmed', 'Khan', 20200317, '4A'),
                        ('Maria', 'Garcia', 20191225, '4B'),
                        ('Liam', 'Wilson', 20200229, '4C'),
                        ('Sophia', 'Martinez', 20191111, '4A'),
                        ('James', 'Anderson', 20191010, '4B')
                    ;";
                studentsCommand.Transaction = transaction;
                int rowsAffected = await studentsCommand.ExecuteNonQueryAsync();

                await using var bookingsCommand = _connection.CreateCommand();
                bookingsCommand.CommandText = @"
                    INSERT INTO Bookings (
                        StudentId,
                        BookingDate,
                        TimeSlot)
                    VALUES
                        (1, 20250915, 1600),
                        (2, 20250915, 1610),
                        (3, 20250915, 1620),
                        (4, 20250915, 1630),
                        (5, 20250915, 1640)
                    ;";
                bookingsCommand.Transaction = transaction;
                rowsAffected += await bookingsCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                return rowsAffected == 15;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding default data to Bookings table");
                throw;
            }
        }

        /// <summary>
        /// Verifies that the record for a given student id is as expected after a test of a method has been run.
        /// </summary>
        /// <param name="studentId">The id number for the given student.</param>
        /// <param name="date">The expected BookingDate value as a string in the format "yyyy-MM-dd".</param>
        /// <param name="time">The expected TimeSlot value as a string in the format "HH-mm". </param>
        public async Task<bool> CheckEntryDetails(int studentId, string date, string time)
        {
            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = @"SELECT StudentId, BookingDate, TimeSlot FROM Bookings WHERE StudentId = @id;";
                command.Parameters.AddWithValue("@id", studentId);
                await using var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(1) != date)
                            return false;
                        if (reader.GetString(2) != time)
                            return false;
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Counts the total number of records in the <c>Bookings</c> table. Used to verify that the expected number of 
        /// records exists in the <c>Bookings</c> table to ensure that methods that create or delete records are working 
        /// and that no unexpected records are present.
        /// </summary>
        /// <returns>The total number of records in the <c>Bookings</c> table.</returns>
        private async Task<int> CountBookings()
        {
            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = $"SELECT COUNT(*) FROM Bookings;";
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting entries in the {TableName} table.", "Bookings");
                throw;
            }
        }
    }
}
