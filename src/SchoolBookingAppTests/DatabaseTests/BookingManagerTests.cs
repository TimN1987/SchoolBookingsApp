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
    }
}
