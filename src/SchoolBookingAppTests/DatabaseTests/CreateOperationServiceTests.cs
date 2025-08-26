using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Struct;
using Serilog;

namespace SchoolBookingAppTests.DatabaseTests
{
    public class CreateOperationServiceTests
    {
        private const string TestConnectionString = "Data Source=:memory:";
        private readonly DatabaseConnectionInformation _connectionInformation;
        private readonly SqliteConnection _connection;
        private readonly DatabaseInitializer _databaseInitializer;
        private readonly string _loggingFilePath;

        public CreateOperationServiceTests()
        {
            _connectionInformation = new DatabaseConnectionInformation();

            _connection = new SqliteConnection(TestConnectionString);
            _connection.Open();

            _databaseInitializer = new DatabaseInitializer(_connection, _connectionInformation);
            _databaseInitializer.InitializeDatabaseAsync().GetAwaiter().GetResult();

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
            Assert.Throws<ArgumentNullException>(() => new CreateOperationService(null!));
        }

        /// <summary>
        /// Checks that a valid <see cref="CreateOperationService">CreateOperationService</see> is created when 
        /// a valid <c>connection</c> is passed as the parameter for the constructor.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameter_CreatesValidCreateOperationService()
        {
            //Arrange & Act
            var createOperationService = new CreateOperationService(_connection);

            //Assert
            Assert.NotNull(createOperationService);
            Assert.IsType<CreateOperationService>(createOperationService);
        }

        //AddParent method tests

        /// <summary>
        /// Tests that the <see cref="CreateOperationService.AddParent"/> method successfully adds data to the 
        /// correct tables when valid inputs are provided. It checks that the method returns <c>true</c> and 
        /// that the data is entered into the "Parents" and "Relationships" tables in the database.
        /// </summary>
        /// <param name="firstName">A valid first name for a parent.</param>
        /// <param name="lastName">A valid last name for a parent.</param>
        /// <param name="relationships">A list of relationships with children for the parent.</param>
        [Theory]
        [MemberData(nameof(AddParentValidMemberData))]
        public async Task AddParent_ValidInputs_ReturnsTrueDataSuccessfullyAdded(
            string firstName,
            string lastName,
            List<(int studentId, string relationship)> relationships
            )
        {
            //Arrange
            await ClearTables();
            await AddTestStudent(); //Ensure students exist in the Students table for the Relationships Foreign Key.
            var createOperationService = new CreateOperationService(_connection);

            //Act
            var dataAdded = await createOperationService.AddParent(firstName, lastName, relationships);

            //Assert
            Assert.True(dataAdded);
            Assert.True(await CheckDataEnteredSuccessfully("Parents"));
            Assert.True(await CheckDataEnteredSuccessfully("ParentStudents"));
        }

        /// <summary>
        /// Tests that the <see cref="CreateOperationService.AddParent"/> method throws an <c>Argument 
        /// Exception</c> if a parameter is null.
        /// </summary>
        [Fact]
        public async Task AddParent_NullInputs_ThrowsArgumentException()
        {
            // Arrange
            await ClearTables();
            var createOperationService = new CreateOperationService(_connection);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await createOperationService.AddParent(null!, null!, null!));
        }

        //AddStudent method tests

        /// <summary>
        /// Tests that the <see cref="CreateOperationService.AddStudent"/> method successfully adds data to 
        /// the <c>Students</c> table when valid inputs are provided. It checks that the method returns 
        /// <c>true</c>.
        /// </summary>
        /// <param name="firstName">A valid first name for a student.</param>
        /// <param name="lastName">A valid last name for a student.</param>
        /// <param name="dateOfBirth">A valid date of birth for a student as an int.</param>
        /// <param name="className">A valid class name for a student.</param>
        [Theory]
        [MemberData(nameof(AddStudentValidMemberData))]
        public async Task AddStudent_ValidInputs_ReturnsTrueDataSuccessfullyAdded(
            string firstName,
            string lastName,
            int dateOfBirth,
            string className
            )
        {
            //Arrange
            await ClearTables();
            var createOperationService = new CreateOperationService(_connection);

            //Act
            var dataAdded = await createOperationService.AddStudent(firstName, lastName, dateOfBirth, className);

            //Assert
            Assert.True(dataAdded);
            Assert.True(await CheckDataEnteredSuccessfully("Students"));
        }

        /// <summary>
        /// Tests that the <see cref="CreateOperationService.AddStudent"/> method throws an <c>Argument 
        /// Exception</c> if a parameter is null.
        /// </summary>
        [Fact]
        public async Task AddStudent_NullInputs_ThrowsArgumentException()
        {
            // Arrange
            await ClearTables();
            var createOperationService = new CreateOperationService(_connection);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await createOperationService.AddStudent(null!, null!, 0, null!));
        }

        //AddData method tests

        /// <summary>
        /// Tests that the <see cref="CreateOperationService.AddData"/> method successfully adds data to the 
        /// Data table in the database when valid inputs are provided. It checks that the method returns 
        /// <c>true</c> and that the data is entered into the "Data" table in the database.
        /// </summary>
        /// <param name="studentData">A record of data for a student.</param>
        [Theory]
        [MemberData(nameof(AddDataValidMemberData))]
        public async Task AddData_ValidInputs_ReturnsTrueDataSuccessfullyAdded(StudentDataRecord studentData)
        {
            //Arrange
            await ClearTables();
            await AddTestStudent(); //Ensure students exist in the Students table for the Foreign Key.
            var createOperationService = new CreateOperationService(_connection);

            //Act
            var dataAdded = await createOperationService.AddData(studentData);

            //Assert
            Assert.True(dataAdded);
            Assert.True(await CheckDataEnteredSuccessfully("Data"));
        }

        //AddComment method tests

        /// <summary>
        /// Tests that the <see cref="CreateOperationService.AddComments"/> method successfully adds comments 
        /// to the Comments table in the database when valid inputs are provided. It checks that the method 
        /// return <c>true</c> and that the data is entered into the "Comments" table in the database.
        /// </summary>
        /// <param name="comments">A record of comments from the meeting for a given student.</param>
        [Theory]
        [MemberData(nameof(AddCommentsValidMemberData))]
        public async Task AddComments_ValidInputs_ReturnsTrueDataSuccessfullyAdded(MeetingCommentsRecord comments)
        {
            //Arrange
            await ClearTables();
            await AddTestStudent(); //Ensure students exist in the Students table for the Foreign Key.
            var createOperationService = new CreateOperationService(_connection);

            //Act
            var dataAdded = await createOperationService.AddComments(comments);

            //Assert
            Assert.True(dataAdded);
            Assert.True(await CheckDataEnteredSuccessfully("Comments"));
        }

        //Member data

        /// <summary>
        /// Provides valid member data for the <see cref="AddParent_ValidInputs_ReturnsTrueDataSuccessfullyAdded"/> test.
        /// </summary>
        /// <returns>A valid first name, last name and list of relationships for a parent.</returns>
        public static IEnumerable<object[]> AddParentValidMemberData()
        {
            yield return new object[]
            {
                "John",
                "Doe",
                new List<(int studentId, string relationship)>
                {
                    (1, "Father"),
                    (2, "Guardian")
                }
            };
            yield return new object[]
            {
                "Jane",
                "Smith",
                new List<(int studentId, string relationship)>
                {
                    (3, "Mother")
                }
            };
            yield return new object[]
            {
                "Alice",
                "Johnson",
                new List<(int studentId, string relationship)>
                {
                    (4, "Aunt"),
                    (5, "Mother")
                }
            };
        }

        /// <summary>
        /// Provides valid member data for the <see cref="AddStudent_ValidInputs_ReturnsTrueDataSuccessfullyAdded"/> 
        /// test.
        /// </summary>
        /// <returns>A valid first name, last name, date of birth and class for a student.</returns>
        public static IEnumerable<object[]> AddStudentValidMemberData()
        {
            yield return new object[]
            {
                "Emily",
                "Brown",
                20000101, // Date of Birth in YYYYMMDD format
                "5A"
            };
            yield return new object[]
            {
                "Michael",
                "Green",
                20030215, // Date of Birth in YYYYMMDD format
                "6B"
            };
            yield return new object[]
            {
                "Sophia",
                "White",
                20040520, // Date of Birth in YYYYMMDD format
                "7C"
            };
        }

        /// <summary>
        /// Provides valid member data for the <see cref="AddData_ValidInputs_ReturnsTrueDataSuccessfullyAdded"/> 
        /// test.
        /// </summary>
        /// <returns>A valid record of the data for a given student.</returns>
        public static IEnumerable<object[]> AddDataValidMemberData()
        {
            yield return new object[]
            {
                new StudentDataRecord
                {
                    StudentId = 1,
                    Math = 5,
                    MathComments = string.Empty,
                    Reading = 3,
                    ReadingComments = "More work needed.",
                    Writing = 4,
                    WritingComments = "Practise handwriting.",
                    Science = 5,
                    History = 3,
                    Geography = 3,
                    MFL = 4,
                    PE = 5,
                    Art = 4,
                    Music = 5,
                    RE = 4,
                    DesignTechnology = 3,
                    Computing = 4
                }
            };
            yield return new object[]
            {
                new StudentDataRecord
                {
                    StudentId = 2,
                    Math = 5,
                    MathComments = string.Empty,
                    Reading = 5,
                    ReadingComments = string.Empty,
                    Writing = 5,
                    WritingComments = string.Empty,
                    Science = 5,
                    MFL = 5,
                    PE = 5,
                    Art = 5,
                    Music = 5,
                    RE = 5,
                    DesignTechnology = 5,
                    Computing = 1
                }
            };
            yield return new object[]
            {
                new StudentDataRecord
                {
                    StudentId = 3,
                    Math = 2,
                    MathComments = "Practise times tables.",
                    Reading = 1,
                    ReadingComments = "Needs to learn letter sounds.",
                    Writing = 1,
                    WritingComments = "Has used some capital letters."
                }
            };
        }

        /// <summary>
        /// Provides valid member data for the <see cref="AddComments_ValidInputs_ReturnsTrueDataSuccessfullyAdded"/> 
        /// test.
        /// </summary>
        /// <returns>A valid record of meeting comments for a given student.</returns>
        public static IEnumerable<object[]> AddCommentsValidMemberData()
        {
            yield return new object[]
            {
                new MeetingCommentsRecord
                {
                    StudentId = 1,
                    GeneralComments = "Good progress overall.",
                    PupilComments = "I enjoy school.",
                    ParentComments = "Very happy with the school.",
                    BehaviorNotes = "Good behavior in class.",
                    AttendanceNotes = "Attendance is excellent.",
                    HomeworkNotes = "Homework is completed on time.",
                    ExtraCurricularNotes = "Participates in sports.",
                    SpecialEducationalNeedsNotes = "No special needs identified.",
                    SafeguardingNotes = "No safeguarding issues.",
                    OtherNotes = "Keep up the good work!",
                    DateAdded = 20231001
                }
            };
            yield return new object[]
            {
                new MeetingCommentsRecord
                {
                    StudentId = 2,
                    GeneralComments = "Needs improvement in math.",
                    PupilComments = "Math is hard for me.",
                    ParentComments = "We are working on it at home.",
                    BehaviorNotes = "Occasional disruptions in class.",
                    AttendanceNotes = "Attendance is good.",
                    HomeworkNotes = "Homework is sometimes late.",
                    ExtraCurricularNotes = "Enjoys music lessons.",
                    SpecialEducationalNeedsNotes = string.Empty,
                    SafeguardingNotes = string.Empty,
                    OtherNotes = "Looking forward to the next meeting.",
                    DateAdded = 20231002
                }
            };
            yield return new object[]
            {
                new MeetingCommentsRecord
                {
                    StudentId = 3,
                    GeneralComments = "Struggling with reading.",
                    PupilComments = "I don't like reading.",
                    ParentComments = "We are trying to encourage reading at home.",
                    BehaviorNotes = "Generally well-behaved.",
                    AttendanceNotes = "Attendance is satisfactory.",
                    HomeworkNotes = "Homework is often incomplete.",
                    ExtraCurricularNotes = "Does not participate in extracurricular activities.",
                    SpecialEducationalNeedsNotes = "Receiving additional support.",
                    SafeguardingNotes = string.Empty,
                    OtherNotes = string.Empty,
                    DateAdded = 20231003
                }
            };
        }

        //Helper methods

        /// <summary>
        /// Clears the tables in the database by deleting all records from the Parents, Students, and 
        /// Relationships tables. Allows for a fresh start for each test run without needing to delete 
        /// the database file itself.
        /// </summary>
        private async Task<bool> ClearTables()
        {
            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = @"
                    DELETE FROM ParentStudents;
                    DELETE FROM Parents;
                    DELETE FROM Students;
                    DELETE FROM Data;
                    DELETE FROM Comments;";
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds test student data to the Students table for testing purposes. This method is used to ensure 
        /// that there are records in the Students table for testing the AddParent method. This ensures that 
        /// the relationships can be established with existing students in the database for the Foreign Key.
        /// </summary>
        /// <returns><c>true</c> if the students are added successfully. <c>false</c> if not.</returns>
        private async Task<bool> AddTestStudent()
        {
            //Default values for testing
            var firstName = "TestFirstName";
            var lastName = "TestLastName";
            var dateOfBirth = 0;
            var className = "TestClass";

            var commandText = @"
            INSERT INTO Students (FirstName, LastName, DateOfBirth, Class)
            VALUES (@FirstName, @LastName, @DateOfBirth, @Class);";

            try
            {
                await using var transaction = _connection.BeginTransaction();

                for (int _ = 0; _ < 5; _++)
                {
                    var command = _connection.CreateCommand();
                    command.CommandText = commandText;
                    command.Transaction = transaction;
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                    command.Parameters.AddWithValue("@Class", className);
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Counts the number of records in a specified table to check if any data has been entered.
        /// </summary>
        /// <param name="table">The table to be checked.</param>
        /// <returns><c>true</c> if data has been entered. <c>false</c> if no data has been entered.</returns>
        /// <remarks>The tables must be cleared before attempting to enter data to ensure a valid check.</remarks>
        private async Task<bool> CheckDataEnteredSuccessfully(string table)
        {
            try
            {
                var commandText = string.Format("SELECT COUNT(*) FROM {0};", table);

                var command = _connection.CreateCommand();
                command.CommandText = commandText;

                return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
