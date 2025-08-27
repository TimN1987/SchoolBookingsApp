using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Factories;

namespace SchoolBookingAppTests.FactoryTests
{
    public class ConnectionFactoryTests
    {
        private const string TestConnectionString = "Data Source=:memory:";
        private const string InvalidConnectionString = "No Date Source";
        private readonly DatabaseConnectionInformation _connectionInformation;
        private readonly ConnectionFactory _connectionFactory;

        public ConnectionFactoryTests()
        {
            _connectionInformation = new DatabaseConnectionInformation();
            _connectionFactory = new ConnectionFactory(_connectionInformation);
        }

        //Constructor tests.
        
        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when a <see langword="null"/> parameter is 
        /// passed to the constructor of the <see cref="ConnectionFactory"/> class. Ensures that the user does not try 
        /// to create a <see cref="ConnectionFactory"/> instance with no connection information.
        /// </summary>
        [Fact]
        public void Constructor_NullParameter_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ConnectionFactory(null!));
        }

        /// <summary>
        /// Verifies that the <see cref="ConnectionFactory"/> constructor creates an object of the expected type that is 
        /// not <see langword="null"/>.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameter_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var connectionFactory = new ConnectionFactory(_connectionInformation);

            //Assert
            Assert.NotNull(connectionFactory);
            Assert.IsType<ConnectionFactory>(connectionFactory);
        }

        //GetConnection tests.

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when a <see langword="null"/> connection string 
        /// is passed as the parameter of the <see cref="ConnectionFactory.GetConnection"/> method. Avoids attempts to 
        /// create a <see cref="SqliteConnection"/> without a valid connection string.
        /// </summary>
        [Fact]
        public async Task GetConnection_NullConnectionString_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _connectionFactory.GetConnection(null!));
        }

        /// <summary>
        /// Verifies that an <see cref="ArgumentException"/> is thrown if an invalid connection string (that does not start 
        /// with 'Data Source=') is passed to the <see cref="ConnectionFactory.GetConnection"/> method. Avoids attempts to 
        /// create a <see cref="SqliteConnection"/> without a valid connection string.
        /// </summary>
        [Fact]
        public async Task GetConnection_InvalidConnectionString_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _connectionFactory.GetConnection(InvalidConnectionString));
        }

        /// <summary>
        /// Verifies that an open <see cref="SqliteConnection"/> with the expected connection string when a valid 
        /// connection string is passed to the <see cref="ConnectionFactory.GetConnection"/> method. Ensures that the 
        /// method can be used to return valid connections.
        /// </summary>
        [Fact]
        public async Task GetConnection_ValidConnectionString_CreatesOpenConnection()
        {
            //Arrange & Act
            await using var connection = await _connectionFactory.GetConnection(TestConnectionString);

            //Assert
            Assert.NotNull(connection);
            Assert.Equal(TestConnectionString, connection.ConnectionString);
            Assert.Equal(ConnectionState.Open, connection.State);
        }

        /// <summary>
        /// Verifies that multiple <see cref="SqliteConnection"/>s can be opened by repeated calls to the <see 
        /// cref="ConnectionFactory.GetConnection"/> method. Ensures that the user can open multiple connections at once, 
        /// for example for running asynchronous database operations in parallel.
        /// </summary>
        [Fact]
        public async Task GetConnection_MultipleCalls_MultipleConnectionsOpened()
        {
            //Arrange & Act
            var tasks = Enumerable.Range(0, 3)
                .Select(_ => _connectionFactory.GetConnection(TestConnectionString));
            var connections = await Task.WhenAll(tasks);

            //Assert
            Assert.All(connections, c =>
            {
                Assert.NotNull(c);
                Assert.Equal(TestConnectionString, c.ConnectionString);
                Assert.Equal(ConnectionState.Open, c.State);
            });
        }
    }
}
