using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.Database;
using Serilog;

namespace SchoolBookingAppTests.DatabaseTests;

public class DeleteOperationServiceTests
{
    private const string TestConnectionString = "Data Source=:memory:";
    private const string InvalidTableName = "InvalidTableName";
    private readonly DatabaseConnectionInformation _connectionInformation;
    private readonly SqliteConnection _connection;
    private readonly DatabaseInitializer _databaseInitializer;
    private readonly DeleteOperationService _deleteOperationService;
    private readonly string _loggingFilePath;

    public DeleteOperationServiceTests()
    {
        _connectionInformation = new DatabaseConnectionInformation();

        _connection = new SqliteConnection(TestConnectionString);
        _connection.Open();

        _databaseInitializer = new DatabaseInitializer(_connection, _connectionInformation);
        _databaseInitializer.InitializeDatabaseAsync().GetAwaiter().GetResult();

        _deleteOperationService = new DeleteOperationService(_connection);

        _loggingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _connectionInformation.ApplicationFolder, "Logs", "CreateOperationServiceTests.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(_loggingFilePath)
            .CreateLogger();
    }

    //Primary constructor tests.

    /// <summary>
    /// Tests that the constructor of DeleteOperationService throws an <see cref="ArgumentNullException">
    /// ArgumentNullExcpetion</see> when a null parameter is passed.
    /// </summary>
    [Fact]
    public void Constructor_NullParameter_ThrowsArgumentNullException()
    {
        //Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeleteOperationService(null!));
    }

    /// <summary>
    /// Tests that the constructor of <see cref="DeleteOperationService">DeleteOperationService</see> creates an instance 
    /// when a valid parameter is passed.
    /// </summary>
    [Fact]
    public void Constructor_ValidParameter_CreatesInstance()
    {
        //Arrange & Act
        var service = new DeleteOperationService(_connection);

        //Assert
        Assert.NotNull(service);
        Assert.IsType<DeleteOperationService>(service);
    }

    //DeleteRecord method tests.

    /// <summary>
    /// Checks that an <see cref="ArgumentNullException"/> is thrown when a null table name is passed to the <see 
    /// cref="DeleteOperationService.DeleteRecord"/> method. The table name must be valid so that the correct data can 
    /// be identified and deleted.
    /// </summary>
    [Fact]
    public async Task DeleteRecord_NullTableName_ThrowsArgumentNullException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _deleteOperationService.DeleteRecord(null!, 1));
    }

    /// <summary>
    /// Checks that an <see cref="ArgumentException"/> is thrown when an empty table name is passed to the <see 
    /// cref="DeleteOperationService.DeleteRecord"/> method. The table name must not be empty to ensure that the 
    /// record can be located and deleted.
    /// </summary>
    [Fact]
    public async Task DeleteRecord_EmptyTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecord(string.Empty, 1));
    }

    /// <summary>
    /// Checks that an <see cref="ArgumentException"/> is thrown when a table name consisting only of whitespace is 
    /// passed to the <see cref="DeleteOperationService.DeleteRecord"/> method. The table name must not be just 
    /// whitespace to ensure that the record can be located and deleted.
    /// </summary>
    [Fact]
    public async Task DeleteRecord_WhiteSpaceTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecord("   ", 1));
    }

    /// <summary>
    /// Checks that an <see cref="ArgumentException"/> is thrown when an invalid table name is passed to the <see 
    /// cref="DeleteOperationService.DeleteRecord"/> method. The table name must exist in the database, so that the 
    /// record can be located and deleted.
    /// </summary>
    [Fact]
    public async Task DeleteRecord_InvalidTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecord(InvalidTableName, 1));
    }

    /// <summary>
    /// Checks that an <see cref="ArgumentException"/> is thrown when an invalid <c>recordId</c> is passed to the 
    /// <see cref="DeleteOperationService.DeleteRecord"/> method with a valid table name. The <c>recordId</c> must be a 
    /// positive integer to match an Id in the database.
    /// </summary>
    [Fact]
    public async Task DeleteRecord_InvalidRecordId_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecord("Students", -1));
    }

    /// <summary>
    /// Checks that the <see cref="DeleteOperationService.DeleteRecord"/> method returns <c>true</c> when a valid table 
    /// name and record Id are passed as parameters. This indicates that the record was successfully deleted. Also checks 
    /// that the number of rows remaining in the database is as expected after the deletion, ensuring that the correct 
    /// number of rows was deleted.
    /// </summary>
    /// <param name="tableName">The name of the table from which the record should be deleted.</param>
    /// <param name="recordId">The Id number that provides the primary key for the record to be deleted.</param>
    /// <param name="expectedRowsRemaining">The number of rows expected to be remaining in all tables after the deletion.</param>
    /// <remarks>All remaining rows in tables are counted to check that any deletions by cascade are checked for.</remarks>
    [Theory]
    [MemberData(nameof(DeleteRecordValidMemberData))]
    public async Task DeleteRecord_ValidInputs_ReturnsTrueCorrectNumberRowsAffected(
        string tableName, int recordId, int expectedRowsRemaining)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act
        var dataUpdated = await _deleteOperationService.DeleteRecord(tableName, recordId);

        //Assert
        Assert.True(dataUpdated);
        Assert.Equal(expectedRowsRemaining, await CountRemainingRows());

    }

    //DeleteRecordsByCriteria method tests.

    /// <summary>
    /// Verifies that the <see cref="DeleteOperationService.DeleteRecordsByCriteria"/> method  throws an <see
    /// cref="ArgumentNullException"/> when the <paramref name="tableName"/> parameter is null. Ensures that the 
    /// method does not try to use a null table name, which would lead to an error when trying to delete records.
    /// </summary>
    [Fact]
    public async Task DeleteRecordsByCriteria_NullTableName_ThrowsArgumentNullException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _deleteOperationService.DeleteRecordsByCriteria(null!, []));
    }

    /// <summary>
    /// Verifies that the <see cref="DeleteOperationService.DeleteRecordsByCriteria"/> method throws an <see
    /// cref="ArgumentException"/> when the <paramref name="tableName"/> parameter is empty. Ensures that invalid 
    /// empty tables names are not accepted, which would lead to an error when trying to delete records.
    /// </summary>
    [Fact]
    public async Task DeleteRecordsByCriteria_EmptyTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecordsByCriteria(string.Empty, []));
    }

    /// <summary>
    /// Verifies that <see cref="DeleteOperationService.DeleteRecordsByCriteria"/> throws an  <see
    /// cref="ArgumentException"/> when the <paramref name="tableName"/> parameter contains only whitespace. Ensures 
    /// that the method does not accept invalid table names that consist solely of whitespace, which would lead to an 
    /// error when trying to delete records.
    /// </summary>
    [Fact]
    public async Task DeleteRecordsByCriteria_WhiteSpaceTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecordsByCriteria("   ", []));
    }

    /// <summary>
    /// Verifies that the <see cref="DeleteOperationService.DeleteRecordsByCriteria"/> method throws an <see 
    /// cref="ArgumentException"/> when an invalid table name is passed. Ensures that the method does not attempt to 
    /// delete records from a table that does not exist, which would lead to an error.
    /// </summary>
    [Fact]
    public async Task DeleteRecordsByCriteria_InvalidTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecordsByCriteria(InvalidTableName, []));
    }

    /// <summary>
    /// Verifies that the <see cref="DeleteOperationService.DeleteRecordsByCriteria(string, object)"/> method throws
    /// an <see cref="ArgumentException"/> when the <paramref name="criteria"/> parameter is <see langword="null"/>. 
    /// Ensure that the method does not attempt to delete records with null criteria, which would lead to an error. 
    /// </summary>
    [Fact]
    public async Task DeleteRecordsByCriteria_NullCriteria_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecordsByCriteria("Students", null!));
    }

    /// <summary>
    /// Verifies that the <see cref="DeleteOperationService.DeleteRecordsByCriteria"/> method throws an <see
    /// cref="ArgumentException"/> when called with an empty criteria list. Ensures that there are valid criteria 
    /// for deleting data to avoid deleting all records in the table.
    /// </summary>
    [Fact]
    public async Task DeleteRecordsByCriteria_EmptyCriteria_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.DeleteRecordsByCriteria("Students", []));
    }

    /// <summary>
    /// Checks that the <see cref="DeleteOperationService.DeleteRecordsByCriteria"/> method returns <c>true</c> when 
    /// valid inputs are passed, indicating that records were successfully deleted based on the criteria. It also 
    /// checks that the number of records remaining in the database matches the expected count after the deletion.
    /// </summary>
    /// <param name="tableName">The name of the table from which records will be removed.</param>
    /// <param name="criteria">A list of criteria that any records must match to be deleted.</param>
    /// <param name="expectedRecordsRemaining">The number of records that should be remaining after the deletion 
    /// based on the default test data.</param>
    [Theory]
    [MemberData(nameof(DeleteRecordsByCriteriaValidMemberData))]
    public async Task DeleteRecordsByCriteria_ValidInputs_ReturnsTrueCorrectNumberOfRecordsRemaining(
        string tableName,
        List<(string, object)> criteria,
        int expectedRecordsRemaining)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act
        var dataRemoved = await _deleteOperationService.DeleteRecordsByCriteria(tableName, criteria);

        //Assert
        Assert.True(dataRemoved);
        Assert.Equal(expectedRecordsRemaining, await CountRemainingRows());
    }

    //ClearTable method tests.

    /// <summary>
    /// Checks that a <see cref="ArgumentNullException"/> is thrown when a null table name is passed to the <see 
    /// cref="DeleteOperationService.ClearTable"/> method. The table name must be valid so that the data can be cleared 
    /// from the correct table.
    /// </summary>
    [Fact]
    public async Task ClearTable_NullTableName_ThrowsArgumentNullException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _deleteOperationService.ClearTable(null!));
    }

    /// <summary>
    /// Checks that a <see cref="ArgumentException"/> is thrown when an empty table name is passed to the <see 
    /// cref="DeleteOperationService.ClearTable"/> method. The table name must be valid so that the data can be cleared 
    /// from the correct table.
    /// </summary>
    [Fact]
    public async Task ClearTable_EmptyTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.ClearTable(string.Empty));
    }

    /// <summary>
    /// Checks that a <see cref="ArgumentException"/> is thrown when a whitespace table name is passed to the <see 
    /// cref="DeleteOperationService.ClearTable"/> method. The table name must be valid so that the data can be cleared 
    /// from the correct table.
    /// </summary>
    [Fact]
    public async Task ClearTable_WhiteSpaceTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.ClearTable("   "));
    }

    /// <summary>
    /// Checks that a <see cref="ArgumentNullException"/> is thrown when an invalid table name is passed to the <see 
    /// cref="DeleteOperationService.ClearTable"/> method. The table name must be valid so that the data can be cleared 
    /// from the correct table.
    /// </summary>
    [Fact]
    public async Task ClearTable_InvalidTableName_ThrowsArgumentException()
    {
        //Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _deleteOperationService.ClearTable(InvalidTableName));
    }

    /// <summary>
    /// Verifies that the <see cref="DeleteOperationService.ClearTable"/> method successfully clears all records
    /// from the specified table and returns <see langword="true"/> when a valid table name is provided. Ensures that 
    /// the method completes successfully and deletes the correct records from the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table to be cleared. Must be a valid table name in the database.</param>
    /// <param name="expectedRecordsRemaining">The expected number of records remaining in the table after the operation. This is used to validate that the
    /// table was cleared correctly.</param>
    [Theory]
    [InlineData("ParentStudents", 6)]
    [InlineData("Parents", 3)]
    [InlineData("Students", 3)]
    public async Task ClearTable_ValidTableName_ReturnsTrueNoRecordsRemain(string tableName, int expectedRecordsRemaining)
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();
        //Act
        var cleared = await _deleteOperationService.ClearTable(tableName);
        //Assert
        Assert.True(cleared);
        Assert.Equal(expectedRecordsRemaining, await CountRemainingRows());
    }


    //ClearAllTables method tests.

    /// <summary>
    /// Checks that the <see cref="DeleteOperationService.ClearAllTables"/> method returns <c>true</c> when it is 
    /// called and that there are no records remaining in the database after the method has run. This indicates that 
    /// all tables were successfully cleared of data. It will return <c>false</c> if an exception was thrown during 
    /// the clearing process, indicating that the operation failed.
    /// </summary>
    [Fact]
    public async Task CLearAllTables_TablesContainData_ReturnsTrueNoRecordsRemain()
    {
        //Arrange
        await ClearTables();
        await AddDefaultData();

        //Act
        var cleared = await _deleteOperationService.ClearAllTables();

        //Assert
        Assert.True(cleared);
        Assert.Equal(0, await CountRemainingRows());
    }

    //MemberData for providing valid inputs to test the Delete methods.

    /// <summary>
    /// Provides valid member data for testing the <see cref="DeleteOperationService.DeleteRecord"/> method. Includes the 
    /// table to delete the record from, the primary key Id value for the record to be deleted, and the expected number 
    /// of rows remaining in all tables after deletion to account for cascade deletions.
    /// </summary>
    public static IEnumerable<object[]> DeleteRecordValidMemberData()
    {
        yield return new object[] {
            "ParentStudents", 1, 9
        };
        yield return new object[] {
            "Parents", 1, 8
        };
        yield return new object[] {
            "Students", 1, 8
        };
        yield return new object[] {
            "Parents", 3, 9
        };
    }

    /// <summary>
    /// Provides valid member data for testing the <see cref="DeleteOperationService.DeleteRecordsByCriteria"/>. 
    /// Includes the table to delete records from, a list of criteria that any records must match to be deleted, and 
    /// the expected number of records remaining in all tables after deletion to account for cascade deletions.
    /// </summary>
    public static IEnumerable<object[]> DeleteRecordsByCriteriaValidMemberData()
    {
        yield return new object[] {
            "ParentStudents", 
            new List<(string, object)> { ("Relationship", "Father") },
            9
        };
        yield return new object[] {
            "ParentStudents", 
            new List<(string, object)> { ("ParentId", 1), ("StudentId", 1) },
            10
        };
        yield return new object[] {
            "ParentStudents", 
            new List<(string, object)> { ("ParentId", 1), ("Relationship", "Father") },
            9
        };
        yield return new object[] {
            "Students", 
            new List<(string, object)> { ("FirstName", "Sally"), ("LastName", "Moon") },
            9
        };
        yield return new object[] {
            "Students", 
            new List<(string, object)> { ("Class", "5B"), ("LastName", "Doe") },
            5
        };
        yield return new object[] {
            "Parents", 
            new List<(string, object)> { ("FirstName", "Simon"), ("LastName", "Doe") },
            8
        };
    }

    //Helper methods.

    /// <summary>
    /// Clears the test database tables to ensure that the expected data can be added before a method is tested. Used to 
    /// ensure consistent testing and avoid unexpected outcomes.
    /// </summary>
    private async Task<bool> ClearTables()
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM ParentStudents;
                DELETE FROM Parents;
                DELETE FROM Students;
                DELETE FROM Data;
                DELETE FROM Comments;";
            await command.ExecuteNonQueryAsync();

            Log.Information("Tables successfully cleared.");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to clear tables.", ex);
            throw;
        }
    }

    /// <summary>
    /// Adds default data to the test database tables. This is used to ensure that the database has the expected data 
    /// at the start of a test, enabling consistent testing and careful tracking of expected deletions.
    /// </summary>
    private async Task<bool> AddDefaultData()
    {
        try
        {
            using var transaction = _connection.BeginTransaction();

            var studentCommand = _connection.CreateCommand();
            studentCommand.CommandText = @"
                INSERT INTO Students (FirstName, LastName, DateOfBirth, Class)
                VALUES
                    ('John', 'Doe', 20191010, '5B'),
                    ('Jane', 'Doe', 20191010, '5B'),
                    ('Sally', 'Moon', 20200101, '4C')
                ;";
            await studentCommand.ExecuteNonQueryAsync();

            var parentsCommand = _connection.CreateCommand();
            parentsCommand.CommandText = @"
                INSERT INTO Parents (FirstName, LastName)
                VALUES
                    ('Simon', 'Doe'),
                    ('Jennifer', 'Doe'),
                    ('Mark', 'Moon')
                ;";
            await parentsCommand.ExecuteNonQueryAsync();

            var relationshipCommand = _connection.CreateCommand();
            relationshipCommand.CommandText = @"
                INSERT INTO ParentStudents (ParentId, StudentId, Relationship)
                VALUES
                    (1, 1, 'Father'),
                    (1, 2, 'Father'),
                    (2, 1, 'Mother'),
                    (2, 2, 'Mother'),
                    (3, 3, 'Grandfather')
                ;";
            await relationshipCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to add default data to test database.", ex);
            throw;
        }
    }

    /// <summary>
    /// Counts the numbers of rows remaining in the tables used for method tests to check that the expected number of 
    /// rows has been deleted during a test.
    /// </summary>
    private async Task<int> CountRemainingRows()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
                SELECT
                    (SELECT COUNT(*) FROM ParentStudents) + 
                    (SELECT COUNT(*) FROM Parents) + 
                    (SELECT COUNT(*) FROM Students)
                ;";
        var remainingRows = await command.ExecuteScalarAsync();
        return Convert.ToInt32(remainingRows);
    }
}
