using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Database;

namespace SchoolBookingApp.MVVM.Factories
{
    /// <summary>
    /// Interface for a factory that provides a connection to the SQLite database.
    /// </summary>
    public interface IConnectionFactory
    {
        Task<SqliteConnection> GetConnection();
        Task<SqliteConnection> GetConnection(string connectionString);
    }

    /// <summary>
    /// Factory class for creating connections to the SQLite database.
    /// </summary>
    public class ConnectionFactory(DatabaseConnectionInformation connectionInformation) : IConnectionFactory
    {
        //Fields
        private readonly DatabaseConnectionInformation _connectionInformation = connectionInformation;
        private readonly string _applicationFolderName = connectionInformation.ApplicationFolder;
        private readonly string _databaseFolderName = connectionInformation.DatabaseFolder;
        private readonly string _databaseName = connectionInformation.DatabaseFileName;

        //Methods

        /// <summary>
        /// Creates a connection to the SQLite database using the default connection information.
        /// </summary>
        public async Task<SqliteConnection> GetConnection()
        {
            return await GetConnection(_connectionInformation.ConnectionString);
        }

        /// <summary>
        /// Creates a connection to the SQLite database using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string for the database.</param>
        /// <exception cref="ArgumentNullException">Thrown if the given connection string is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown if the given connection string is not in the correct 
        /// format.</exception>
        /// <remarks>Can be used for testing with the connection string "Data Source=:memory:".</remarks>
        public async Task<SqliteConnection> GetConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
            if (!connectionString.StartsWith("Data Source="))
                throw new ArgumentException("Connection string must start with 'Data Source='.", nameof(connectionString));

            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
