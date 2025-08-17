using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Enums;
using Serilog;

namespace SchoolBookingAppTests.DatabaseTests
{
    public class ReadOperationServiceTests
    {
        private const string TestConnectionString = "Data Source=:memory:";
        private const int TotalNameRecords = 20;

        private const string InvalidTableName = "InvalidTableName";
        private const string ValidTableName = "Students";
        private const string InvalidFieldName = "InvalidFieldName";
        private const string ValidKeyword = "Doe";
        private const string WhiteSpace = " ";

        private readonly DatabaseConnectionInformation _connectionInformation;
        private readonly SqliteConnection _connection;
        private readonly SqliteConnection _closedConnection;
        private readonly DatabaseInitializer _databaseInitializer;
        private readonly ReadOperationService _readOperationService;
        private readonly string _loggingFilePath;

        public ReadOperationServiceTests()
        {
            _connectionInformation = new DatabaseConnectionInformation();

            _connection = new SqliteConnection(TestConnectionString);
            _connection.Open();

            _closedConnection = new SqliteConnection(TestConnectionString);

            _databaseInitializer = new DatabaseInitializer(_connection, _connectionInformation);
            _databaseInitializer.InitializeDatabaseAsync().GetAwaiter().GetResult();

            _readOperationService = new ReadOperationService(_connection);

            _loggingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _connectionInformation.ApplicationFolder, "Logs", "CreateOperationServiceTests.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(_loggingFilePath)
                .CreateLogger();
        }

        //Constructor tests.

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
                new ReadOperationService(null!));
        }

        /// <summary>
        /// Verifies that an instance of <see cref="ReadOperationService"/> is successfully created  when a valid
        /// connection is provided to its constructor. Ensures that no exceptions are thrown and that the created instance 
        /// is not null and is of the correct type.
        /// </summary>
        [Fact]
        public void Constructor_ValidConnection_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var readOperationService = new ReadOperationService(_connection);

            //Assert
            Assert.NotNull(readOperationService);
            Assert.IsType<ReadOperationService>(readOperationService);
        }

        /// <summary>
        /// Verifies that an <see cref="InvalidOperationException"/> is thrown if a closed connection is passed as the 
        /// parameter of the Constructor. Ensures that the <see cref="ReadOperationService"/> is not relying on a 
        /// closed connection to read from the database and that the connection is handled separately.
        /// </summary>
        [Fact]
        public void Constructor_ClosedConnection_ThrowsInvalidOperationException()
        {
            //Arrange, Act & Assert
            Assert.Throws<InvalidOperationException>(() => new ReadOperationService(_closedConnection));
        }

        //GetAllSearchData tests.

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.GetAllSearchData"/> method returns a non-null list of 
        /// <see cref="SearchResult"/> objects when the database contains data with the correct number of records.
        /// </summary>
        [Fact]
        public async Task GetAllSearchData_TablesContainData_ReturnsCorrectNumberOfRecords()
        {
            //Arrange
            await ClearTables();
            await AddDefaultData();

            //Act
            var result = await _readOperationService.GetAllSearchData();

            //Assert
            Assert.NotNull(result);
            Assert.Equal(TotalNameRecords, result.Count);
            Assert.Contains(result, r => r.LastName == "Doe");
        }

        //SearchByKeyword tests.

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method throws an <see 
        /// cref="ArgumentNullException"/>" if a null keyword is passed as a parameter. This ensures that the method does 
        /// not try to execute a search with an invalid keyword, which would lead to unexpected behavior or errors.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_NullKeyword_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _readOperationService.SearchByKeyword(null!));
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method returns a list of all the names 
        /// from the database when the <paramref name="keyword"/> parameter is an empty string. This ensures that the 
        /// default behavior of the search method is to return all data when no specific keyword is provided, which allows 
        /// the user to see all available names in the database before filtering by a specific keyword.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_EmptyKeyword_ReturnsAllData()
        {
            //Arrange
            await ClearTables();
            await AddDefaultData();

            //Act
            var result = await _readOperationService.SearchByKeyword(string.Empty);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(TotalNameRecords, result.Count);
            Assert.Contains(result, r => r.LastName == "Doe");
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method returns an empty list when the 
        /// <paramref name="keyword"/> parameter is a whitespace string. This ensures that the method does not return any 
        /// results when the keyword is not meaningful, which would lead to unexpected behavior or errors in the UI.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_WhiteSpaceKeyword_ReturnsEmptyList()
        {
            //Arrange & Act
            var result = await _readOperationService.SearchByKeyword(WhiteSpace);

            //Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method throws an <see 
        /// cref="ArgumentException"/> if an empty table name is passed as a parameter. This ensures that the method 
        /// does not attempt to execute a search on an invalid table, which would lead to unexpected behavior or errors.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_EmptyTableName_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _readOperationService.SearchByKeyword(ValidKeyword, string.Empty));
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method throws an <see 
        /// cref="ArgumentException"/> if a whitespace table name is passed as a parameter. This ensures that the method 
        /// does not attempt to execute a search on an invalid table, which would lead to unexpected behavior or errors.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_WhiteSpaceTableName_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _readOperationService.SearchByKeyword(ValidKeyword, WhiteSpace));
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method throws an <see 
        /// cref="ArgumentException"/> if an invalid table name is passed as a parameter. This ensures that the method 
        /// does not attempt to execute a search on an invalid table, which would lead to unexpected behavior or errors.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_InvalidTableName_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _readOperationService.SearchByKeyword(ValidKeyword, InvalidTableName));
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method throws an <see 
        /// cref="ArgumentException"/> if an empty field is passed as a parameter. This ensures that the method does not 
        /// attempt to execute an invalid search on a field, which would lead to unexpected behavior or errors.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_EmptyField_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _readOperationService.SearchByKeyword(ValidKeyword, ValidTableName, string.Empty));
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method throws an <see 
        /// cref="ArgumentException"/> if a whitespace field is passed as a parameter. This ensures that the method does 
        /// not attempt to execute an invalid search on a field, which would lead to unexpected behavior or errors.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_WhiteSpaceField_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _readOperationService.SearchByKeyword(ValidKeyword, ValidTableName, WhiteSpace));
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method throws an <see 
        /// cref="ArgumentException"/> if an invalid field is passed as a parameter. This ensures that the method does not 
        /// attempt to execute an invalid search on a field, which would lead to unexpected behavior or errors.
        /// </summary>
        [Fact]
        public async Task SearchByKeyword_InvalidField_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _readOperationService.SearchByKeyword(ValidKeyword, ValidTableName, InvalidFieldName));
        }

        [Theory]
        [InlineData("Doe", "Parents", "LastName", 2)]
        [InlineData("Doe", "Students", "LastName", 2)]
        [InlineData("Sally", "Students", "FirstName", 1)]
        [InlineData("J", "Students", "FirstName", 3)]
        [InlineData("Jones", null, "LastName", 2)]
        [InlineData("S", "Parents", null, 3)]
        [InlineData("Doe", null, null, 4)]
        public async Task SearchByKeyword_ValidParameters_ReturnsExpectedResults(
            string keyword, string tableName, string field, int expectedResultsCount)
        {
            //Arrange
            await ClearTables();
            await AddDefaultData();

            //Act
            var result = await _readOperationService.SearchByKeyword(keyword, tableName, field);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResultsCount, result.Count);
            Assert.All(result, r => Assert.Contains(keyword, r.FirstName + r.LastName));
        }

        //SearchByCriteria tests.

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByCriteria"/> method returns an empty list when the
        /// criteria parameter is null. This ensures that if no criteria are specified, no valid results are returned.
        /// </summary>
        [Fact]
        public async Task SearchByCriteria_NullCriteria_ReturnsEmptyList()
        {
            //Arrange & Act
            var result = await _readOperationService.SearchByCriteria(null!);

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByCriteria"/> method returns an empty list when no 
        /// criteria are provided. This ensures that if no criteria are specified, no valid results are returned.
        /// </summary>
        [Fact]
        public async Task SearchByCriteria_NoCriteria_ReturnsNoData()
        {
            //Arrange & Act
            var result = await _readOperationService.SearchByCriteria([]);

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(SearchByCriteriaValidMemberData))]
        public async Task SearchByCriteria_ValidCriteria_ReturnsExpectedResults(
            List<SearchCriteria> criteria, int expectedResultsCount, List<string> expectedFirstNames)
        {
            //Arrange
            await ClearTables();
            await AddDefaultData();

            //Act
            var searchResults = await _readOperationService.SearchByCriteria(criteria);

            //Assert
            Assert.NotNull(searchResults);
            Assert.Equal(expectedResultsCount, searchResults.Count);

            foreach (var name in expectedFirstNames)
                Assert.NotEmpty(searchResults.Where(result => result.FirstName == name));
        }



        //Member data for tests.
        public static IEnumerable<object[]> SearchByCriteriaValidMemberData()
        {
            yield return new object[]
            {
                new List<SearchCriteria>
                {
                    new SearchCriteria(
                        Field: DatabaseField.FirstName,
                        SQLOperator.Equals,
                        [ "John"]
                        ),
                    new SearchCriteria(
                        Field: DatabaseField.LastName,
                        SQLOperator.Equals,
                        [ "Doe" ]
                        )
                },
                1,
                new List<string> { "John" }
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
                    ('Sally', 'Moon', 20200101, '4C'),
                    ('Jackie', 'Smith', 20200101, '4C'),
                    ('Ibrahim', 'Khan', 20200101, '4C'),
                    ('Aisha', 'Ali', 20200101, '3B'),
                    ('Liam', 'Johnson', 20200101, '3B'),
                    ('Emma', 'Williams', 20200101, '2A'),
                    ('Olivia', 'Brown', 20200101, '2A'),
                    ('Noah', 'Jones', 20200101, '1B')
                ;";
                await studentCommand.ExecuteNonQueryAsync();

                var parentsCommand = _connection.CreateCommand();
                parentsCommand.CommandText = @"
                INSERT INTO Parents (FirstName, LastName)
                VALUES
                    ('Simon', 'Doe'),
                    ('Jennifer', 'Doe'),
                    ('Mark', 'Moon'),
                    ('Lisa', 'Smith'),
                    ('Ali', 'Khan'),
                    ('Fatima', 'Ali'),
                    ('John', 'Johnson'),
                    ('Sarah', 'Williams'),
                    ('Michael', 'Brown'),
                    ('Emily', 'Jones')
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
                    (3, 3, 'Grandfather'),
                    (4, 4, 'Aunt'),
                    (5, 5, 'Uncle'),
                    (6, 6, 'Mother'),
                    (7, 7, 'Father'),
                    (8, 8, 'Mother'),
                    (9, 9, 'Father'),
                    (10, 10, 'Mother')
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
    }
}
