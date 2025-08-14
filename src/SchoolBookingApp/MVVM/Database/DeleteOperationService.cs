using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Serilog;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Defines a contract for a service that performs delete operations on a database.
    /// </summary>
    public interface IDeleteOperationService
    {
        Task<bool> DeleteRecord(string tableName, int recordId);
        Task<bool> DeleteRecordsByCriteria(string tableName, List<(string, object)> criteria);
        Task<bool> ClearTable(string tableName);
        Task<bool> ClearAllTables();
    }

    /// <summary>
    /// A service for performing delete operations on a database using an SqliteConnection. Includes methods for deleting 
    /// a record from a given table, for deleting records by a given criteria and for clearing a table.
    /// </summary>
    /// <param name="connection">A valid <see cref="SqliteConnection"/>SqliteConnection to the database.</param>
    public class DeleteOperationService(SqliteConnection connection) : IDeleteOperationService
    {
        private readonly SqliteConnection _connection = connection
            ?? throw new ArgumentNullException(nameof(connection));

        private readonly HashSet<string> _validTables = [
            "Students", "Parents", "ParentStudents", "Data", "Comments"
        ];

        private readonly string _deleteRecordQuery = "DELETE FROM {0} WHERE {1} = {2};";
        private readonly string _deleteRecordsByCriteria = "DELETE FROM {0} WHERE 1 = 1";
        private readonly string _deleteCriterion = " AND {0} = {1}";
        private readonly string _clearTableQuery = "DELETE FROM {0};";


        //Methods

        /// <summary>
        /// Deletes the record from the given table where the record ID matches the specified value. Used to delete a single 
        /// record from a table based on the primary key.
        /// </summary>
        /// <param name="tableName">The table name containing the record to be deleted.</param>
        /// <param name="recordId">The value of the primary key for the record to be deleted.</param>
        /// <returns><c>true</c> if the record is successfully deleted, <c>false</c> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the tableName is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown if the recordId or tableName are invalid.</exception>
        public async Task<bool> DeleteRecord(string tableName, int recordId)
        {
            ArgumentNullException.ThrowIfNull(tableName);
            if (!_validTables.Contains(tableName))
                throw new ArgumentException($"Invalid table name: {tableName}. Valid tables are: {string.Join(", ", _validTables)}");
            if (recordId <= 0)
                throw new ArgumentException("Record ID must be greater than zero.");

            try
            {
                var dataDeleted = tableName == "ParentStudents" ?
                    await ExecuteDeletion(tableName, "ParentId", recordId)
                    : await ExecuteDeletion(tableName, "Id", recordId);

                return dataDeleted;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting record from table {TableName} with ID {RecordId}", tableName, recordId);
                return false;
            }
        }

        /// <summary>
        /// Deletes all records from a given table that match one or more specified criteria. This method allows for 
        /// targeted deletions based on multiple conditions, making it flexible for various use cases.
        /// </summary>z
        /// <param name="tableName">The table from which the records should be deleted.</param>
        /// <param name="criteria">A list of criteria that records to be deleted must meet. Contains value tuples of a 
        /// string representing the column name and an object representing the desired value.</param>
        /// <returns><see langword="true"/> if the records are successfully deleted, else <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the given table name is null.</exception>
        /// <exception cref="ArgumentException">Thrown if an invalid table name or invalid criteria are given.</exception>
        public async Task<bool> DeleteRecordsByCriteria(string tableName, List<(string, object)> criteria)
        {
            ArgumentNullException.ThrowIfNull(tableName);
            if (!_validTables.Contains(tableName))
                throw new ArgumentException($"Invalid table name: {tableName}. Valid tables are: {string.Join(", ", _validTables)}");
            if (criteria == null || criteria.Count == 0)
                throw new ArgumentException("Criteria cannot be null or empty.");
            try
            {
                var commandText = new StringBuilder(string.Format(_deleteRecordsByCriteria, tableName));

                foreach (var criterion in criteria)
                {
                    if (string.IsNullOrWhiteSpace(criterion.Item1))
                        continue;
                    commandText.AppendFormat(_deleteCriterion, criterion.Item1, "@value_" + criterion.Item1);
                }

                commandText.Append(';');

                using var command = _connection.CreateCommand();
                command.CommandText = commandText.ToString();

                foreach (var criterion in criteria)
                {
                    if (string.IsNullOrWhiteSpace(criterion.Item1))
                        continue;
                    command.Parameters.AddWithValue("@value_" + criterion.Item1, criterion.Item2);
                }

                await command.ExecuteNonQueryAsync();

                Log.Information("Successfully deleted records from table {TableName} with criteria {@Criteria}", tableName, criteria);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting records from table {TableName} with criteria {@Criteria}", tableName, criteria);
                return false;
            }
        }

        /// <summary>
        /// Clears all data from the specified table in the database. This operation deletes all records with the given 
        /// table name, effectively resetting it to an empty state.
        /// </summary>
        /// <param name="tableName">The name of the table from which the data will be cleared. Must be a valid table name 
        /// from the database.</param>
        /// <returns><see langword="true"/> if data is successfully deleted, else <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if the input is null. The table naem cannot be null.</exception>
        /// <exception cref="ArgumentException">Throw if an invalid table name is entered. Data cannot be deleted from a 
        /// tabled that does not exist in the database.</exception>
        public async Task<bool> ClearTable(string tableName)
        {
            ArgumentNullException.ThrowIfNull(tableName);
            if (!_validTables.Contains(tableName))
                throw new ArgumentException($"Invalid table name: {tableName}. Valid tables are: {string.Join(", ", _validTables)}");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = string.Format(_clearTableQuery, tableName);
                await command.ExecuteNonQueryAsync();

                Log.Information("Successfully cleared table {TableName}", tableName);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error clearing table {TableName}", tableName);
                return false;
            }
        }

        /// <summary>
        /// Clears the data from all tables in the database. This operation deletes all records from each table, returning 
        /// the database to an empty state. It is useful for resetting the database during development or testing.
        /// </summary>
        /// <returns><see langword="true"/> if the data is successfully deleted, else <see langword="false"/>.</returns>
        public async Task<bool> ClearAllTables()
        {
            try
            {
                using var transaction = _connection.BeginTransaction();

                foreach (var table in _validTables)
                {
                    var commandText = string.Format(_clearTableQuery, table);
                    using var command = _connection.CreateCommand();
                    command.CommandText = commandText;
                    command.Transaction = transaction;
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                Log.Information("Successfully cleared all tables in the database.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error clearing all tables in the database.");
                return false;
            }
        }

        //Helper methods

        /// <summary>
        /// Executes a deletion operation on the specified table using the provided criteria and value.
        /// </summary>
        /// <param name="tableName">The name of the table to delete the record from.</param>
        /// <param name="criteria">The column to base the criteria on.</param>
        /// <param name="value">The desired value for the criteria.</param>
        /// <returns><c>true</c> if the record is successfully deleted, else <c>false</c>.</returns>
        private async Task<bool> ExecuteDeletion(string tableName, string criteria, object value)
        {
            try
            {
                var commandText = string.Format(_deleteRecordQuery, tableName, criteria, "@value");

                using var command = _connection.CreateCommand();
                command.CommandText = commandText;
                command.Parameters.AddWithValue("@value", value);
                await command.ExecuteNonQueryAsync();

                Log.Information("Successfully deleted records from table {TableName} with criteria {Criteria} and value {Value}",
                    tableName, criteria, value);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting records from table {TableName} with criteria {Criteria} and value {Value}",
                    tableName, criteria, value);
                return false;
            }
        }
    }
}
