using Microsoft.Data.Sqlite;
using Serilog;
using Serilog.Sinks.File;
using SchoolBookingApp.MVVM.Database;
using System.IO;

namespace SchoolBookingAppTests.DatabaseTests
{
    public class DatabaseInitializerTests
    {
        private const string TestConnectionString = "Data Source=:memory:";
        private readonly DatabaseConnectionInformation _connectionInformation;
        private readonly SqliteConnection _connection;
        private readonly string _loggingFilePath;

        public DatabaseInitializerTests()
        {
            _connectionInformation = new DatabaseConnectionInformation
            {
                ApplicationFolder = "SchoolBookingTest",
                DatabaseFolder = "TestDatabase",
                DatabaseFileName = "TestDatabase.db"
            };

            _connection = new SqliteConnection(TestConnectionString);
            _connection.Open();

            _loggingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _connectionInformation.ApplicationFolder, "Logs", "DatabaseInitializerTests.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(_loggingFilePath)
                .CreateLogger();
        }

        //Constructor tests for the Primary Constructor.

        /// <summary>
        /// Checks that an <c>Argument Null Exception</c> is thrown when a <c>null</c> reference is passed as 
        /// the parameter for the constructor of <see cref="DatabaseInitializer">DatabaseInitializer</c>.
        /// </summary>
        [Fact]
        public void Constructor_NullParameter_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DatabaseInitializer(null!, null!));
        }

        /// <summary>
        /// Checks that a valid <see cref="DatabaseInitializer">DatabaseInitializer</see> is created when valid 
        /// <c>connection information</c> is passed as the parameter for the constructor."/>
        /// </summary>
        [Fact]
        public void Constructor_ValidParameter_CreatesValidDatabaseInitializer()
        {
            //Arrange & Act
            var databaseInitializer = new DatabaseInitializer(_connection, _connectionInformation);

            //Assert
            Assert.NotNull(databaseInitializer);
            Assert.IsType<DatabaseInitializer>(databaseInitializer);
        }

        //InitializeDatabaseAsync method tests.

        /// <summary>
        /// Checks that the <see cref="DatabaseInitializer.InitializeDatabaseAsync">InitializeDatabaseAsync</see> 
        /// method returns <c>true</c> when called with valid connection information, indicating that the 
        /// database has been successfully initialized.
        /// </summary>
        [Fact]
        public async Task InitializeDatabaseAsync_ValidConnection_ReturnsTrue()
        {
            // Arrange
            var databaseInitializer = new DatabaseInitializer(_connection, _connectionInformation);
            // Act
            var result = await databaseInitializer.InitializeDatabaseAsync();
            // Assert
            Assert.True(result);
        }
    }
}