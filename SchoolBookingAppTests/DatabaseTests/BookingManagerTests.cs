using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SchoolBookingApp.Database;
using Serilog;

namespace SchoolBookingAppTests.DatabaseTests;

public class BookingManagerTests
{
    private const string TestConnectionString = "Data Source=:memory:";
    private const int InitialTotalRecords = 5;
    private const int TotalRecordsAfterOneInsertion = 6;
    private const int TotalRecordsAfterOneDeletion = 4;

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
    [MemberData(nameof(CreateUpdateBookingInvalidMemberData))]
    public async Task CreateBooking_InvalidBookingInformation_ThrowsArgumentException(Booking invalidBookingInformation)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _bookingManager.CreateBooking(invalidBookingInformation));
    }

    /// <summary>
    /// Verifies that a booking is successfully created when valid booking information is provided to the <see 
    /// cref="BookingManager.CreateBooking"/> method. Uses member data to provide a range of valid data with different 
    /// student Ids, dates and times using the different constructors of the <see cref="Booking"/>. Ensures that the 
    /// method successfully creates exactly one new record in the <c>Bookings</c> table with the expected details.
    /// </summary>
    /// <param name="validBookingInformation">A <see cref="Booking"/> record containing a valid student id for a 
    /// student who exists in the <c>Students</c> table and a unique, valid booking date and time.</param>
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

    //UpdateBooking tests.

    /// <summary>
    /// Verifies that an <see cref="ArgumentException"/> is thrown when invalid booking information is provided to the 
    /// <see cref="BookingManager.UpdateBooking(Booking)"/> method. Uses member data to provide a range of invalid 
    /// data with cases where one or more elements should throw an <see cref="ArgumentException"/>. Ensures that any 
    /// invalid data is correctly identified and handled, preventing invalid records from being created in the database 
    /// or displayed to the user.
    /// </summary>
    /// <param name="invalidBookingInformation">A <see cref="Booking"/> record containing invalid data.</param>
    [Theory]
    [MemberData(nameof(CreateUpdateBookingInvalidMemberData))]
    public async Task UpdateBooking_InvalidBookingInformation_ThrowsArgumentException(
        Booking invalidBookingInformation)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _bookingManager.UpdateBooking(invalidBookingInformation));
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentException"/> is thrown when valid booking information is provided to the 
    /// <see cref="BookingManager.UpdateBooking"/> method but where the student Id does not have an existing booking. 
    /// Ensures that an attempt to update a non-existent booking is correctly identified and handled, preventing any 
    /// issues when attempting to update records in the database.
    /// </summary>
    /// <param name="bookingInformation">A <see cref="Booking"/> <see langword="struct"/> containing a valid 
    /// student id and a unique, valid booking date and time.</param>
    [Theory]
    [MemberData(nameof(UpdateBookingNoBookingToUpdateMemberData))]
    public async Task UpdateBooking_NoExistingBooking_ThrowsArgumentException(Booking bookingInformation)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _bookingManager.UpdateBooking(bookingInformation));
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentException"/> is thrown when valid booking information is provided to the 
    /// <see cref="BookingManager.UpdateBooking"/> method but where the time slot clashes with an existing booking for 
    /// a different student on the same date. Ensures that an attempt to update a booking to a time slot that is already 
    /// booked is correctly identified and handled, preventing double-booking and ensuring the integrity of the 
    /// booking system.
    /// </summary>
    /// <param name="bookingInformation">A <see cref="Booking"/> <see langword="struct"/> containing valid booking 
    /// data with a valid booking for the given student id, but where the selected time slot on the given date is 
    /// already booked.</param>
    [Theory]
    [MemberData(nameof(UpdateBookingsTimeSlotClashMemberData))]
    public async Task UpdateBooking_TimeSlotClash_ThrowsArgumentException(Booking bookingInformation)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _bookingManager.UpdateBooking(bookingInformation));
    }

    /// <summary>
    /// Verifies that a booking is successfully updated when valid booking information is provided to the <see 
    /// cref="BookingManager.UpdateBooking"/> method. Uses member data to provide a range of valid data with student 
    /// ids that already have a booking and different dates and times using the different constructors of the <see 
    /// cref="Booking"/> <see langword="struct"/>. Ensures that the method successfully updates the selected records 
    /// with date and time values that do not clash with any other existing bookings.
    /// </summary>
    /// <param name="updatedBookingInformation">A <see cref="Booking"/> <see langword="struct"/> containing a valid 
    /// student id which already has a booking in the <c>Bookings</c> table and a valid booking date and time slot 
    /// that have not already been booked.</param>
    [Theory]
    [MemberData(nameof(UpdateBookingValidMemberData))]
    public async Task UpdateBooking_ValidBookingInformation_UpdatesTheRecordSuccessfully(
        Booking updatedBookingInformation)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act
        var result = await _bookingManager.UpdateBooking(updatedBookingInformation);

        //Assert
        Assert.True(result);
        Assert.Equal(InitialTotalRecords, await CountBookings());
        Assert.Equal(
            updatedBookingInformation.DateString,
            await RetrieveBookingDate(updatedBookingInformation.StudentId));
        Assert.Equal(
            updatedBookingInformation.TimeString,
            await RetrieveTimeSlot(updatedBookingInformation.StudentId));
    }

    //DeleteBooking tests.

    /// <summary>
    /// Verifies that an <see cref="ArgumentException"/> is thrown when an invalid <paramref name="studentId"/> is 
    /// passed as the parameter of the <see cref="BookingManager.DeleteBooking"/> method. A <paramref name="studentId"/> 
    /// is invalid if it is <= 0 or if there is not already a booking with that <paramref name="studentId"/> in the 
    /// <c>Bookings</c> table.
    /// </summary>
    /// <param name="studentId">The StudentId field for the record to be deleted.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(-5)]
    [InlineData(100)]
    public async Task DeleteBooking_InvalidStudentId_ThrowsArgumentException(int studentId)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => 
            await _bookingManager.DeleteBooking(studentId));
    }

    /// <summary>
    /// Verifies that a record is successfully deleted when a valid <paramref name="studentId"/> is passed as the 
    /// parameter to the <see cref="BookingManager.DeleteBooking"/> method. A valid <paramref name="studentId"/> must 
    /// be > 0 and already have a record in the <c>Bookings</c> table to delete.
    /// </summary>
    /// <param name="studentId">The StudentId field for the record to be deleted.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task DeleteBooking_ValidStudentId_ExactlyOneRecordDeleted(int studentId)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act
        bool result = await _bookingManager.DeleteBooking(studentId);

        //Assert
        Assert.True(result);
        Assert.Equal(TotalRecordsAfterOneDeletion, await CountBookings());
        Assert.Equal(0, await CountRecordsById(studentId));
    }

    //ListBookings tests.

    /// <summary>
    /// Verifies that an empty list is returned when the <see cref="BookingManager.ListBookings"/> method is called 
    /// with an empty <c>Bookings</c> table.
    /// </summary>
    [Fact]
    public async Task ListBookings_EmptyTable_ReturnsEmptyList()
    {
        //Arrange
        await ClearTables();

        //Act
        List<Booking> result = await _bookingManager.ListBookings();

        //Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that all records from the <c>Bookings</c> table are returned when the <see 
    /// cref="BookingManager.ListBookings"/> method is called.
    /// </summary>
    /// <param name="expectedResults">A list containing <see cref="Booking"/> <see langword="struct"/>s for all the 
    /// records in the <c>Bookings</c> table to compare to the actual results.</param>
    [Theory]
    [MemberData(nameof(ListBookingsExpectedResultsMemberData))]
    public async Task ListBookings_BookingsInTable_ReturnsAllBookings(List<Booking> expectedResults)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act
        List<Booking> results = await _bookingManager.ListBookings();

        //Assert
        Assert.NotNull(results);
        Assert.Equal(InitialTotalRecords, results.Count);
        Assert.Equal(expectedResults, results);
    }

    //RetrieveBookingInformation tests.

    /// <summary>
    /// Verifies that the <see cref="BookingManager.RetrieveBookingInformation"/> throws an <see 
    /// cref="ArgumentException"/> when an invalid student it is passed as the argument. An invalid student id could be 
    /// <= 0 or a student id that does not exist in the <c>Students</c> table. Used to check that no search is 
    /// performed for a student that does not exist.
    /// </summary>
    /// <param name="studentId">The student id for a student that does not exist.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(20)]
    [InlineData(-5)]
    [InlineData(11)]
    public async Task RetrieveBookingInformation_InvalidStudentIdInput_ThrowsArgumentException(int studentId)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => 
            await _bookingManager.RetrieveBookingInformation(studentId));
    }

    /// <summary>
    /// Verifies that the <see cref="BookingManager.RetrieveBookingInformation"/> method throws an <see 
    /// cref="InvalidOperationException"/> if a valid student id is passed but where there is no booking in the
    /// <c>Bookings</c> table to retrieve. Used to check that the method does not try to retrieve booking information 
    /// for a booking that does not exist.
    /// </summary>
    /// <param name="studentId">The student id for a valid student who does not have a booking.</param>
    [Theory]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public async Task RetrieveBookingInformation_NoBookingForStudentId_ThrowsInvalidOperationException(int studentId)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _bookingManager.RetrieveBookingInformation(studentId));
    }

    /// <summary>
    /// Verifies that the <see cref="BookingManager.RetrieveBookingInformation"/> method correctly retrieves all the 
    /// booking information for a valid, given student id. Checks that the returned <see cref="Booking"/> <see 
    /// langword="struct"/> matches the expected booking information. Used to help the user retrieve all relevant 
    /// booking information for a selected student.
    /// </summary>
    /// <param name="studentId">The student id for the selected student.</param>
    /// <param name="expectedBooking">A <see cref="Booking"/> <see langword="struct"/> containing the expected 
    /// data to be retrieved from the database. Used to compare to the actual data returned.</param>
    [Theory]
    [MemberData(nameof(RetrieveBookingInformationValidMemberData))]
    public async Task RetrieveBookingInformation_ValidStudentIdInput_ReturnsCorrectBookingInformation(
        int studentId, Booking expectedBooking)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act
        Booking booking = await _bookingManager.RetrieveBookingInformation(studentId);

        //Assert
        Assert.Equal(expectedBooking, booking);
    }

    //ClearBookings tests.

    /// <summary>
    /// Verifies that the <see cref="BookingManager.ClearBookings"/> method deletes all records from the <c>Bookings</c> 
    /// table. Checks that the method returns true and that the table is empty after the method is called. Ensures that 
    /// the <c>Bookings</c> table can be cleared for new bookings, for example before a new Parents' Evening.
    /// </summary>
    [Fact]
    public async Task ClearBookings_BookingsTableExists_NoRecordsLeftInTable()
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act
        bool result = await _bookingManager.ClearBookings();

        //Assert
        Assert.True(result);
        Assert.Equal(0, await CountBookings());
    }

    //Member Data

    /// <summary>
    /// Provides member data for the <see cref="CreateBooking_InvalidBookingInformation_ThrowsArgumentException"/> test 
    /// and the <see cref="UpdateBooking_InvalidBookingInformation_ThrowsArgumentException"/> test. Includes a range of 
    /// invalid data with cases where one or more elements should throw an <see cref="ArgumentException"/>. This 
    /// includes values <= 0, dates and times in an invalid format and a student Id that is not found in the 
    /// <c>Students</c> table.
    /// </summary>
    public static IEnumerable<object[]> CreateUpdateBookingInvalidMemberData()
    {
        yield return new object[] { new Booking(-1, string.Empty, string.Empty, new DateTime(2025, 09, 14), new TimeSpan(16, 0, 0)) };
        yield return new object[] { new Booking(11, string.Empty, string.Empty, new DateTime(2025, 09, 14), new TimeSpan(16, 0, 0)) };
        yield return new object[] { new Booking(1, string.Empty, string.Empty, null, new TimeSpan(16, 0, 0)) };
        yield return new object[] { new Booking(1, string.Empty, string.Empty, new DateTime(2025, 09, 14), null) };
        yield return new object[] { new Booking(-5, null!, null!) };
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

    /// <summary>
    /// Provides member data for the <see cref="UpdateBooking_NoExistingBooking_ThrowsArgumentException"/> test. 
    /// Includes a range of valid data with different student Ids, dates and times using the different constructors of 
    /// the <see cref="Booking"/> <see langword="struct"/> but where the student Id does not have an existing booking 
    /// in the <c>Bookings</c> table. Ensures that an <see cref="ArgumentException"/> is thrown when attempting to 
    /// update a booking that does not exist.
    /// </summary>
    public static IEnumerable<object[]> UpdateBookingNoBookingToUpdateMemberData()
    {
        yield return new object[] { new Booking(6, string.Empty, string.Empty, new DateTime(2025, 09, 14), new TimeSpan(16, 0, 0)) };
        yield return new object[] { new Booking(7, "2025-10-01", "17:30") };
        yield return new object[] { new Booking(8, new DateTime(2025, 11, 20, 15, 45, 0)) };
        yield return new object[] { new Booking(9, string.Empty, string.Empty, new DateTime(2025, 12, 05), new TimeSpan(18, 15, 0)) };
        yield return new object[] { new Booking(10, string.Empty, string.Empty, new DateTime(2026, 01, 10), new TimeSpan(16, 30, 0)) };
    }

    /// <summary>
    /// Provides member data for the <see cref="UpdateBooking_TimeSlotClash_ThrowsInvalidOperationException"/> test. 
    /// Includes a range of valid data with different student Ids, dates and times using the different constructors of 
    /// the <see cref="Booking"/> <see langword="struct"/> but where the time slot clashes with an existing booking for 
    /// a different student on the same date. Ensures that an <see cref="ArgumentException"/> is thrown when attempting 
    /// to update a booking to a time slot that is already booked.
    /// </summary>
    public static IEnumerable<object[]> UpdateBookingsTimeSlotClashMemberData()
    {
        yield return new object[] { new Booking(1, string.Empty, string.Empty, new DateTime(2025, 09, 15), new TimeSpan(16, 10, 0)) };
        yield return new object[] { new Booking(2, "2025-09-15", "16:20") };
        yield return new object[] { new Booking(3, new DateTime(2025, 09, 15, 16, 30, 0)) };
        yield return new object[] { new Booking(4, string.Empty, string.Empty, new DateTime(2025, 09, 15), new TimeSpan(16, 40, 0)) };
        yield return new object[] { new Booking(5, string.Empty, string.Empty, new DateTime(2025, 09, 15), new TimeSpan(16, 00, 0)) };
    }

    /// <summary>
    /// Provides member data for the <see cref="UpdateBooking_ValidBookingInformation_UpdatesTheRecordSuccessfully"/> 
    /// test. Ensures that the selected record is updated with the correct new booking date and time slot. Includes a 
    /// range of valid data with different student Ids, dates and times using the different constructors of the 
    /// <see cref="Booking"/> <see langword="struct"/>.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> UpdateBookingValidMemberData()
    {
        yield return new object[] { new Booking(1, string.Empty, string.Empty, new DateTime(2025, 09, 14), new TimeSpan(16, 0, 0)) };
        yield return new object[] { new Booking(2, string.Empty, string.Empty, new DateTime(2025, 10, 01), new TimeSpan(17, 30, 0)) };
        yield return new object[] { new Booking(3, string.Empty, string.Empty, new DateTime(2025, 11, 20), new TimeSpan(15, 45, 0)) };
        yield return new object[] { new Booking(4, string.Empty, string.Empty, new DateTime(2025, 12, 05), new TimeSpan(18, 15, 0)) };
        yield return new object[] { new Booking(5, string.Empty, string.Empty, new DateTime(2026, 01, 10), new TimeSpan(16, 30, 0)) };
        yield return new object[] { new Booking(1, "2025-09-14", "16:00") };
        yield return new object[] { new Booking(2, "2025-10-01", "17:30") };
        yield return new object[] { new Booking(3, "2025-11-20", "15:45") };
        yield return new object[] { new Booking(4, "2025-12-05", "18:15") };
        yield return new object[] { new Booking(5, "2026-01-10", "16:30") };
        yield return new object[] { new Booking(1, new DateTime(2025, 09, 14, 16, 0, 0)) };
        yield return new object[] { new Booking(2, new DateTime(2025, 10, 01, 17, 30, 0)) };
        yield return new object[] { new Booking(3, new DateTime(2025, 11, 20, 15, 45, 0)) };
        yield return new object[] { new Booking(4, new DateTime(2025, 12, 05, 18, 15, 0)) };
        yield return new object[] { new Booking(5, new DateTime(2025, 01, 11, 15, 00, 0)) };
    }

    /// <summary>
    /// Provides member data for the <see cref="ListBookings_BookingsInTable_ReturnsAllBookings"/> test. Used to check 
    /// that the correct list of <see cref="Booking"/> <see langword="struct"/>s is returned when the <see 
    /// cref="BookingManager.ListBookings"/> method is called.
    /// </summary>
    public static IEnumerable<object[]> ListBookingsExpectedResultsMemberData()
    {
        yield return new object[] { new List<Booking>
            { 
                new(1, "John", "Doe", new DateTime(2025, 9, 15), new TimeSpan(16, 0, 0)),
                new(2, "Jane", "Smith", new DateTime(2025, 9, 15), new TimeSpan(16, 10, 0)),
                new(3, "Alice", "Johnson", new DateTime(2025, 9, 15), new TimeSpan(16, 20, 0)),
                new(4, "Bob", "Brown", new DateTime(2025, 9, 15), new TimeSpan(16, 30, 0)),
                new(5, "Charlie", "Davis", new DateTime(2025, 9, 15), new TimeSpan(16, 40, 0))
            }
        };
    }

    /// <summary>
    /// Provides member data for the <see cref="RetrieveBookingInformation_ValidStudentIdInput_ReturnsCorrectBookingInformation"/> 
    /// test. Used to check that all the correct booking information is retrieved from the database for the given student id 
    /// when the <see cref="BookingManager.RetrieveBookingInformation"/> method is called.
    /// </summary>
    public static IEnumerable<object[]> RetrieveBookingInformationValidMemberData()
    {
        yield return new object[] { 
            1, new Booking(1, "John", "Doe", new DateTime(2025, 9, 15), new TimeSpan(16, 0, 0))
        };
        yield return new object[] {
            2, new Booking(2, "Jane", "Smith", new DateTime(2025, 9, 15), new TimeSpan(16, 10, 0))
        };
        yield return new object[] {
            3, new Booking(3, "Alice", "Johnson", new DateTime(2025, 9, 15), new TimeSpan(16, 20, 0))
        };
        yield return new object[] {
            4, new Booking(4, "Bob", "Brown", new DateTime(2025, 9, 15), new TimeSpan(16, 30, 0))
        };
        yield return new object[] {
            5, new Booking(5, "Charlie", "Davis", new DateTime(2025, 9, 15), new TimeSpan(16, 40, 0))
        };
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
                        (1, '2025-09-15', '16:00'),
                        (2, '2025-09-15', '16:10'),
                        (3, '2025-09-15', '16:20'),
                        (4, '2025-09-15', '16:30'),
                        (5, '2025-09-15', '16:40')
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

    /// <summary>
    /// Retrieves the booking date for a given student id. Used to verify that the booking date is as expected after 
    /// an update operation.
    /// </summary>
    /// <param name="studentId">The student id for the record that has been updated.</param>
    private async Task<string> RetrieveBookingDate(int studentId)
    {
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = @"
                    SELECT BookingDate 
                    FROM Bookings 
                    WHERE StudentId = @id;";
            command.Parameters.AddWithValue("@id", studentId);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }
        catch
        {
            Log.Error("An error occurred while retrieving the booking date.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the booking time for a given student id. Used to verify that the booking time is as expected after 
    /// an update operation.
    /// </summary>
    /// <param name="studentId">The student id for the record that has been updated.</param>
    private async Task<string> RetrieveTimeSlot(int studentId)
    {
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = @"
                    SELECT TimeSlot 
                    FROM Bookings 
                    WHERE StudentId = @id;";
            command.Parameters.AddWithValue("@id", studentId);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }
        catch
        {
            Log.Error("An error occurred while retrieving the booking time slot.");
            throw;
        }
    }

    /// <summary>
    /// Counts how many records are in the <c>Bookings</c> table with a given student id. Used to check whether any 
    /// records for the given student id are still remaining after a deletion or that there is only one record with 
    /// the given student id after a record is created or updated.
    /// </summary>
    /// <param name="studentId">The student id that any matching records must have in the StudentId field.</param>
    /// <returns>The number of records with the given student id field.</returns>
    private async Task<int> CountRecordsById(int studentId)
    {
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM Bookings WHERE StudentId = @id;";
            command.Parameters.AddWithValue("@id", studentId);
            var result = await command.ExecuteScalarAsync();
            var count = Convert.ToInt32(result);

            return count;
        }
        catch
        {
            Log.Error("An error occurred while counting bookings with the StudentId: " + studentId);
            throw;
        }
    }
}
