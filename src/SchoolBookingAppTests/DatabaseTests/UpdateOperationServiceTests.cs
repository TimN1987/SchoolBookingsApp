using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Struct;
using Serilog;

namespace SchoolBookingAppTests.DatabaseTests
{
    public class UpdateOperationServiceTests
    {
        private const string TestConnectionString = "Data Source=:memory:";
        private readonly DatabaseConnectionInformation _connectionInformation;
        private readonly SqliteConnection _connection;
        private readonly DatabaseInitializer _databaseInitializer;
        private readonly UpdateOperationService _updateOperationService;
        private readonly string _loggingFilePath;

        public UpdateOperationServiceTests()
        {
            _connectionInformation = new DatabaseConnectionInformation();

            _connection = new SqliteConnection(TestConnectionString);
            _connection.Open();

            _databaseInitializer = new DatabaseInitializer(_connection, _connectionInformation);
            _databaseInitializer.InitializeDatabaseAsync().GetAwaiter().GetResult();

            _updateOperationService = new UpdateOperationService(_connection);

            _loggingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _connectionInformation.ApplicationFolder, "Logs", "CreateOperationServiceTests.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(_loggingFilePath)
                .CreateLogger();
        }

        //Constructor tests for the Primary Constructor.

        /// <summary>
        /// Checks that an <c>Argument Null Exception</c> is thrown when a <c>null</c> reference is passed as 
        /// the constructor parameter.
        /// </summary>
        [Fact]
        public void Constructor_NullParameter_ThrowsArgumentNullExceptionException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UpdateOperationService(null!));
        }

        /// <summary>
        /// Checks that a valid <see cref="UpdateOperationService">UpdateOperationService</see> is created when a  
        /// valid <c>connection</c> is passed as the parameter for the constructor.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameter_CreatesValidCreateOperationService()
        {
            //Arrange & Act
            var updateOperationService = new UpdateOperationService(_connection);

            //Assert
            Assert.NotNull(updateOperationService);
            Assert.IsType<UpdateOperationService>(updateOperationService);
        }

        //UpdateParent method tests

        /// <summary>
        /// Checks that the <see cref="UpdateOperationService.UpdateParent"/> method returns <c>true</c> when 
        /// the parent information is updated.
        /// </summary>
        /// <param name="parentId">The parent id of the parent to be updated.</param>
        /// <param name="firstName">The updated first name if given.</param>
        /// <param name="lastName">The updated last name if given.</param>
        /// <param name="studentRelationships">A list of updated student relationships if given.</param>
        [Theory]
        [MemberData(nameof(UpdateParentValidMemberData))]
        public async Task UpdateParent_ValidInputs_ReturnsTrue(
            int parentId,
            string firstName,
            string lastName,
            List<(int studentId, string relationship)> studentRelationships)
        {
            // Arrange
            await CLearTables();
            await AddTestStudents();
            await AddTestParents();

            // Act
            var dataUpdated = await _updateOperationService.UpdateParent(parentId, firstName, lastName, studentRelationships);

            // Assert
            Assert.True(dataUpdated);
        }

        /// <summary>
        /// Checks that the <see cref="UpdateOperationService.UpdateParent"/> method throws a <see 
        /// cref="ArgumentException"/> if the parameters are passed as <c>null</c>.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateParent_InvalidParentId_ThrowsArgumentException()
        {
            // Arrange, Act & Assert  
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _updateOperationService.UpdateParent(-1));
        }

        //UpdateStudent method tests

        /// <summary>
        /// Checks that the <see cref="UpdateOperationService.UpdateStudent"/> method returns <c>true</c> when 
        /// data in the <c>Student</c> table is updated.
        /// </summary>
        /// <param name="studentId">The student Id number.</param>
        /// <param name="firstName">The first name to be updated if given.</param>
        /// <param name="lastName">The last name to be updated if given.</param>
        /// <param name="dateOfBirth">The date of birth to be updated if given.</param>
        /// <param name="className">The class name to be updated if given.</param>
        [Theory]
        [MemberData(nameof(UpdateStudentValidMemberData))]
        public async Task UpdateStudent_ValidInputs_ReturnsTrue(
            int studentId,
            string? firstName,
            string? lastName,
            int? dateOfBirth,
            string? className)
        {
            //Arrange
            await CLearTables();
            await AddTestStudents();

            //Act
            var dataUpdated = await _updateOperationService.UpdateStudent(studentId, firstName, lastName, dateOfBirth, className);

            //Assert
            Assert.True(dataUpdated);
        }

        /// <summary>
        /// Checks that a <see cref="ArgumentException"/> is thrown when an invalid student Id is passed as a 
        /// parameter.
        /// </summary>
        [Fact]
        public async Task UpdateStudent_InvalidStudentid_ThrowsArgumetnException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _updateOperationService.UpdateStudent(-1));
        }

        //UpdateData method tests

        [Theory]
        [MemberData(nameof(UpdateDataValidMemberData))]
        public async Task UpdateData_ValidInput_ReturnsTrue(StudentDataRecord data)
        {
            //Arrange
            await CLearTables();
            await AddTestStudents();
            await AddTestData();

            // Act
            var dataUpdated = await _updateOperationService.UpdateData(data);

            //Assert
            Assert.True(dataUpdated);
        }

        /// <summary>
        /// Checks that the <see cref="UpdateOperationService.UpdateData"/> method throws an <see 
        /// cref="ArgumentException"/> if an invalid student id is entered./>
        /// </summary>
        [Fact]
        public async Task UpdateData_InvalidStudentId_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _updateOperationService.UpdateData(new StudentDataRecord { StudentId = -1 }));
        }

        //UpdateComments method tests

        /// <summary>
        /// Checks that the <see cref="UpdateOperationService.UpdateComments"/> method returns <c>true</c> when a valid 
        /// set of comments are updated in the <c>Comments</c> table.
        /// </summary>
        /// <param name="comments">A record of comments to be updated for the given student.</param>
        [Theory]
        [MemberData(nameof(UpdateCommentsValidMemberData))]
        public async Task UpdateComments_ValidInputs_ReturnsTrue(MeetingCommentsRecord comments)
        {
            //Arrange
            await CLearTables();
            await AddTestStudents();
            await AddTestComments();

            //Act
            var dataUpdated = await _updateOperationService.UpdateComments(comments);

            //Assert
            Assert.True(dataUpdated);
        }

        /// <summary>
        /// Checks that the <see cref="UpdateOperationService.UpdateComments"/> method throws an <see 
        /// cref="ArgumentException"/> if an invalid student id is entered.
        /// </summary>
        [Fact]
        public async Task UpdateComments_InvalidStudentId_ThrowsArgumentException()
        {
            //Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
               await _updateOperationService.UpdateComments(new MeetingCommentsRecord {  StudentId = -1 }));
        }

        //Member data

        /// <summary>
        /// Provides valid data for testing the <see cref="UpdateOperationService.UpdateParent"/> method.
        /// </summary>
        public static IEnumerable<object[]> UpdateParentValidMemberData()
        {
            yield return new object[] {
                1,
                "Tom",
                "Doe",
                new List<(int studentId, string relationship)> { (1, "Father"), (2, "Uncle") }
            };
            yield return new object[] {
                2,
                "Jenny",
                "Smith",
                null! // No student relationships to update.
            };
            yield return new object[] {
                3,
                "Alanna",
                null!, //No update to last name.
                new List<(int studentId, string relationship)> { (3, "Guardian") }
            };
        }

        /// <summary>
        /// Provides valid data for testing the <see cref="UpdateOperationService.UpdateStudent"/> method.
        /// </summary>
        public static IEnumerable<object[]> UpdateStudentValidMemberData()
        {
            yield return new object[]
            {
                1,
                "Tom",
                "Doe",
                20000101,
                "Class A"
            };
            yield return new object[]
            {
                2,
                "Sandy",
                null!, //No update to last name.
                null!, //No update to date of birth.
                "3BD"
            };
            yield return new object[]
            {
                3,
                null!, //No update to first name.
                "Smith",
                20000202,
                null! //No update to class.
            };
        }

        /// <summary>
        /// Provides valid data for testing the <see cref="UpdateOperationService.UpdateData"/> method.
        /// </summary>
        public static IEnumerable<object[]> UpdateDataValidMemberData()
        {
            yield return new object[]
            {
                new StudentDataRecord
                {
                    StudentId = 1,
                    Math = 85,
                    MathComments = "Good progress",
                    Reading = 90,
                    ReadingComments = "Excellent reading skills",
                    Writing = 88,
                    WritingComments = "Very good writing"
                }
            };
            yield return new object[]
            {
                new StudentDataRecord
                {
                    StudentId = 2,
                    MathComments = string.Empty,
                    ReadingComments = string.Empty,
                    WritingComments = string.Empty,
                    Science = 75,
                    History = 70,
                    Geography = 65,
                    MFL = 60,
                    PE = 85,
                    Art = 80,
                    Music = 75
                }
            };
            yield return new object[]
            {
                new StudentDataRecord
                {
                    StudentId = 3,
                    Math = 92,
                    MathComments = "Outstanding performance",
                    Reading = 95,
                    ReadingComments = "Exceptional reading skills",
                    Writing = 90,
                    WritingComments = "Excellent writing",
                    Science = 88,
                    History = 85,
                    Geography = 80
                }
            };
        }

        /// <summary>
        /// Provides valid data for testing the <see cref="UpdateOperationService.UpdateComments"/> method.
        /// </summary>
        public static IEnumerable<object[]> UpdateCommentsValidMemberData()
        {
            yield return new object[]
            {
                new MeetingCommentsRecord
                {
                    StudentId = 1,
                    GeneralComments = "A great start to the year.",
                    PupilComments = "I love learning PE.",
                    ParentComments = "He is very happy.",
                    BehaviorNotes = "No issues.",
                    AttendanceNotes = "100%",
                    HomeworkNotes = "Always completed on time.",
                    ExtraCurricularNotes = "n/a",
                    SpecialEducationalNeedsNotes = "n/a",
                    SafeguardingNotes = "n/a",
                    OtherNotes = "Enjoys reading.",
                    DateAdded = 20230110
                }
            };
            yield return new object[]
            {
                new MeetingCommentsRecord
                {
                    StudentId = 2,
                    GeneralComments = "Needs to improve in Maths.",
                    PupilComments = "I find Maths difficult.",
                    ParentComments = "We are working on it at home.",
                    BehaviorNotes = "Occasional disruptions.",
                    AttendanceNotes = "95%",
                    HomeworkNotes = "Sometimes late.",
                    OtherNotes = "Likes Science.",
                    DateAdded = 20230215
                }
            };
            yield return new object[]
            {
                new MeetingCommentsRecord
                {
                    StudentId = 3,
                    PupilComments = "I am enjoying my time at school."
                }
            };
        }

        //Helper methods

        /// <summary>
        /// Clears all the tables in the database to ensure a clean state for testing.
        /// </summary>
        /// <returns><c>true</c> if the tables are successfully cleared, else <c>false</c>.</returns>
        private async Task<bool> CLearTables()
        {
            try
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = @"
                DELETE FROM ParentStudents;
                DELETE FROM Parents;
                DELETE FROM Students;
                DELETE FROM Data;
                DELETE FROM Comments;";
                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error clearing tables.");
                throw;
            }
        }

        /// <summary>
        /// Adds test students to the database for testing purposes. This method is called before running 
        /// the <c>UpdateParents</c> method to ensure that relationships can be succesfully added to the 
        /// <c>ParentStudents</c> table with the appropriate Foreign Key constraints.
        /// </summary>
        /// <returns><c>true</c> if the students are correctly added. Else <c>false</c>.</returns>
        private async Task<bool> AddTestStudents()
        {
            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Students (FirstName, LastName, DateOfBirth, Class)
                VALUES
                ('John', 'Doe', 20000101, 'Class A'),
                ('Jane', 'Smith', 20000202, 'Class B'),
                ('Alice', 'Johnson', 20000303, 'Class C');";
                var rowsChanged = await command.ExecuteNonQueryAsync();

                if (rowsChanged <= 0)
                    throw new Exception("No rows were changed when adding test students.");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding test students.");
                throw;
            }
        }

        /// <summary>
        /// Adds test parents to the database for testing purposes. This method is called before running 
        /// the <see cref="UpdateOperationService.UpdateParent"/> method to ensure that parents exist to be
        /// updated.
        /// </summary>
        /// <returns><c>true</c> if the test parents are successfully created, else <c>false</c>.</returns>
        private async Task<bool> AddTestParents()
        {
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Parents (FirstName, LastName)
                VALUES
                ('John', 'Doe'),
                ('Jane', 'Smith'),
                ('Alice', 'Johnson');";
                var rowsChanged = await command.ExecuteNonQueryAsync();

                if (rowsChanged <= 0)
                    throw new Exception("No rows were changed when adding test parents.");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding test parents.");
                throw;
            }
        }

        /// <summary>
        /// Adds test data to the <c>Data</c> table for testing purposes. This method is called before running 
        /// the <c>UpdateData</c> method tests to ensure that there is data to update.
        /// </summary>
        private async Task<bool> AddTestData()
        {
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Data (StudentId, Math, MathComments, Reading, ReadingComments, Writing, WritingComments, Science, History, Geography, MFL, PE, Art, Music, RE, DesignTechnology, Computing)
                VALUES
                (1, 85, 'Good progress', 90, 'Excellent reading skills', 88, 'Very good writing', 80, 75, 70, 65, 90, 95, 80, 85, 90, 42),
                (2, 78, 'Needs improvement', 82, 'Good reading', 80, 'Average writing', 75, 70, 65, 60, 85, 80, 75, 70, 99, 26),
                (3, 92, 'Outstanding performance', 95, 'Exceptional reading skills', 90, 'Excellent writing', 88, 85, 80, 74, 10, 47, 31, 88, 99, 10);";
                var rowsChanged = await command.ExecuteNonQueryAsync();

                if (rowsChanged <= 0)
                    throw new Exception("No rows were changed when adding test data.");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding test data.");
                throw;
            }
        }

        /// <summary>
        /// Adds test comments to the <c>Comments</c> table for testing purposes. This method is called before testing the 
        /// <see cref="UpdateOperationService.UpdateComments"/> method.
        /// </summary>
        /// <returns><c>true</c> if the comments are successfully added, else <c>false</c>.</returns>
        private async Task<bool> AddTestComments()
        {
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Comments (
                        StudentId, 
                        GeneralComments, 
                        PupilComments, 
                        ParentComments, 
                        BehaviorNotes, 
                        AttendanceNotes, 
                        HomeworkNotes, 
                        ExtraCurricularNotes, 
                        SpecialEducationalNeedsNotes, 
                        SafeguardingNotes, 
                        OtherNotes,
                        DateAdded)
                    VALUES (
                        1, 
                        'General comment 1', 
                        'Pupil comment 1', 
                        'Parent comment 1', 
                        'Behavior notes 1', 
                        'Attendance notes 1', 
                        'Homework notes 1', 
                        'Extra curricular notes 1', 
                        'SEN notes 1', 
                        'Safeguarding notes 1', 
                        'Other notes 1',
                        20230101
                    ),
                    (
                        2, 
                        'General comment 2', 
                        'Pupil comment 2', 
                        'Parent comment 2', 
                        'Behavior notes 2', 
                        'Attendance notes 2', 
                        'Homework notes 2', 
                        'Extra curricular notes 2', 
                        'SEN notes 2', 
                        'Safeguarding notes 2', 
                        'Other notes 2',
                        20230202
                    ),
                    (
                        3, 
                        'General comment 3', 
                        'Pupil comment 3', 
                        'Parent comment 3', 
                        'Behavior notes 3', 
                        'Attendance notes 3', 
                        'Homework notes 3', 
                        'Extra curricular notes 3', 
                        'SEN notes 3', 
                        'Safeguarding notes 3', 
                        'Other notes 3',
                        20230303
                    );";
                var rowsChanged = await command.ExecuteNonQueryAsync();

                if (rowsChanged <= 0)
                    throw new Exception("No rows were changed when adding test comments.");

                return true;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding test comments.");
                throw;
            }
        }
    }
}
