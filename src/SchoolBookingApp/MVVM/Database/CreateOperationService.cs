using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Struct;
using Microsoft.Data.Sqlite;
using Serilog;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Defines a contract for creating records in the database, such as adding parents, students, data records, 
    /// and comments.
    /// </summary>
    public interface ICreateOperationService
    {
        Task<bool> AddParent(string firstName, string lastName, List<(int studentId, string relationship)> studentRelationships);
        Task<bool> AddStudent(string firstName, string lastName, int dateOfBirth, string className);
        Task<bool> AddData(StudentDataRecord dataRecord);
        Task<bool> AddComments(MeetingCommentsRecord comments);
    }

    /// <summary>
    /// A service for creating records in the database. It provides methods to add parents, students, student 
    /// data and comments to the bookings database.
    /// </summary>
    /// <param name="connectionInformation"></param>
    public class CreateOperationService(SqliteConnection connection) : ICreateOperationService
    {
        private readonly SqliteConnection _connection = connection
            ?? throw new ArgumentNullException("Connection cannot be null.");

        //SQL command strings for creating records in the database.
        private readonly string _addParentQuery = @"
            INSERT INTO Parents (FirstName, LastName)
            VALUES (@FirstName, @LastName);";
        private readonly string _addParentStudentQuery = @"
            INSERT INTO ParentStudents (ParentId, StudentId, Relationship)
            VALUES (
                @ParentId,
                @StudentId,
                @Relationship
            );";
        private readonly string _addStudentQuery = @"
            INSERT INTO Students (FirstName, LastName, DateOfBirth, Class)
            VALUES (@FirstName, @LastName, @DateOfBirth, @Class);";
        private readonly string _addDataQuery = @"
            INSERT INTO Data (
                StudentId, 
                Math, 
                MathComments, 
                Reading, 
                ReadingComments, 
                Writing,
                WritingComments,
                Science,
                History,
                Geography,
                MFL,
                PE,
                Art,
                Music,
                DesignTechnology,
                Computing,
                RE
            )
            VALUES (
                @StudentId,
                @Math,
                @MathComments,
                @Reading, 
                @ReadingComments, 
                @Writing,
                @WritingComments,
                @Science,
                @History,
                @Geography,
                @MFL,
                @PE,
                @Art,
                @Music,
                @DesignTechnology,
                @Computing,
                @RE
            );";
        private readonly string _addCommentQuery = @"
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
                DateAdded
            )
            VALUES (
                @StudentId,
                @GeneralComments,
                @PupilComments,
                @ParentComments,
                @BehaviorNotes,
                @AttendanceNotes,
                @HomeworkNotes,
                @ExtraCurricularNotes,
                @SpecialEducationalNeedsNotes,
                @SafeguardingNotes,
                @OtherNotes,
                @DateAdded
            );";

        //Methods
        /// <summary>
        /// Adds a parent to the Parents table in the database and their relationships to any children to the 
        /// ParentStudents table.
        /// </summary>
        /// <param name="firstName">The parent's first name.</param>
        /// <param name="lastName">The parent's last name.</param>
        /// <param name="studentRelationships">A list of the relationships the parent has to any children.</param>
        /// <returns>True if the parent is added successfully. False if the operation failed.</returns>
        /// <remarks>The student must already be added to the <c>Student</c> table to be assigned a 
        /// <c>StudentId</c>.</remarks>
        public async Task<bool> AddParent(string firstName, string lastName, List<(int studentId, string relationship)> studentRelationships)
        {
            if (string.IsNullOrEmpty(firstName)
                || string.IsNullOrEmpty(lastName)
                || studentRelationships == null || studentRelationships.Count == 0)
                throw new ArgumentException("Parameters cannot be null, whitespace or empty.");
            
            try
            {
                await using var transaction = _connection.BeginTransaction();

                var parentsCommand = _connection.CreateCommand();
                parentsCommand.CommandText = _addParentQuery;
                parentsCommand.Transaction = transaction;
                parentsCommand.Parameters.AddWithValue(@"FirstName", firstName);
                parentsCommand.Parameters.AddWithValue(@"LastName", lastName);
                var rowsChangedParents = await parentsCommand.ExecuteNonQueryAsync();

                if (rowsChangedParents <= 0) return false;

                var command = _connection.CreateCommand();
                command.CommandText = "SELECT last_insert_rowid()";
                command.Transaction = transaction;
                var parentId = (long)(await command.ExecuteScalarAsync() ?? 0);

                foreach (var relationship in studentRelationships)
                {
                    var parentStudentsCommand = _connection.CreateCommand();
                    parentStudentsCommand.CommandText = _addParentStudentQuery;
                    parentStudentsCommand.Transaction = transaction;
                    parentStudentsCommand.Parameters.AddWithValue(@"ParentId", parentId);
                    parentStudentsCommand.Parameters.AddWithValue(@"StudentId", relationship.studentId);
                    parentStudentsCommand.Parameters.AddWithValue(@"Relationship", relationship.relationship);
                    var rowsChangedParentStudents = await parentStudentsCommand.ExecuteNonQueryAsync();

                    if (rowsChangedParentStudents <= 0) return false;
                }

                await transaction.CommitAsync();
                Log.Information("Parent {FirstName} {LastName} added to database with relationships.", firstName, lastName);
                return true;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Error adding parent to database.");
                return false;
            }
        }

        /// <summary>
        /// Adds a student to the <c>Students</c> table in the database.
        /// </summary>
        /// <param name="firstName">The student's first name.</param>
        /// <param name="lastName">The student's last name.</param>
        /// <param name="dateOfBirth">The student's date of birth in seconds as UNIX time stamp.</param>
        /// <param name="className">The name of the student's class.</param>
        /// <returns><c>true</c> if the record is successfully created. <c>false</c> if the attempt failed.</returns>
        public async Task<bool> AddStudent(string firstName, string lastName, int dateOfBirth, string className)
        {
            if (string.IsNullOrEmpty(firstName)
                || string.IsNullOrEmpty(lastName)
                || dateOfBirth < 0
                || string.IsNullOrEmpty(className))
                throw new ArgumentException("Parameters cannot be null, whitespace or empty.");

            try
            {
                await using var command = new SqliteCommand(_addStudentQuery, _connection);
                command.Parameters.AddWithValue(@"FirstName", firstName);
                command.Parameters.AddWithValue(@"LastName", lastName);
                command.Parameters.AddWithValue(@"DateOfBirth", dateOfBirth);
                command.Parameters.AddWithValue(@"Class", className);

                await command.ExecuteNonQueryAsync();
                Log.Information("Student {FirstName} {LastName} added to database.", firstName, lastName);
                return true;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Error adding student to database.");
                return false;
            }
        }

        /// <summary>
        /// Adds a record of student data to the <c>Data</c> table in the database.
        /// </summary>
        /// <param name="studentData">A struct containing the academic and pastoral data for the student.</param>
        /// <returns><c>true</c> if the record is successfully created. <c>false</c> if the attempt failed.</returns>
        public async Task<bool> AddData(StudentDataRecord studentData)
        {
            try
            {
                await using var command = new SqliteCommand(_addDataQuery, _connection);
                command.Parameters.AddWithValue(@"StudentId", studentData.StudentId);
                command.Parameters.AddWithValue(@"Math", studentData.Math);
                command.Parameters.AddWithValue(@"MathComments", studentData.MathComments);
                command.Parameters.AddWithValue(@"Reading", studentData.Reading);
                command.Parameters.AddWithValue(@"ReadingComments", studentData.ReadingComments);
                command.Parameters.AddWithValue(@"Writing", studentData.Writing);
                command.Parameters.AddWithValue(@"WritingComments", studentData.WritingComments);
                command.Parameters.AddWithValue(@"Science", studentData.Science);
                command.Parameters.AddWithValue(@"History", studentData.History);
                command.Parameters.AddWithValue(@"Geography", studentData.Geography);
                command.Parameters.AddWithValue(@"MFL", studentData.MFL);
                command.Parameters.AddWithValue(@"PE", studentData.PE);
                command.Parameters.AddWithValue(@"Art", studentData.Art);
                command.Parameters.AddWithValue(@"Music", studentData.Music);
                command.Parameters.AddWithValue(@"DesignTechnology", studentData.DesignTechnology);
                command.Parameters.AddWithValue(@"Computing", studentData.Computing);
                command.Parameters.AddWithValue(@"RE", studentData.RE);

                await command.ExecuteNonQueryAsync();
                Log.Information("Student data for StudentId {StudentId} added to database.", studentData.StudentId);
                return true;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Error adding student data to database.");
                throw;
            }
        }

        /// <summary>
        /// Adds comments from parents' meetings to the <c>Comments</c> table in the database.
        /// </summary>
        /// <param name="comments">A struct containing comments from the meeting.</param>
        /// <returns><c>true</c> if the record is successfully created. <c>false</c> if the attempt failed.</returns>
        public async Task<bool> AddComments(MeetingCommentsRecord comments)
        {
            try
            {
                using var command = new SqliteCommand(_addCommentQuery, _connection);
                command.Parameters.AddWithValue(@"StudentId", comments.StudentId);
                command.Parameters.AddWithValue(@"GeneralComments", comments.GeneralComments);
                command.Parameters.AddWithValue(@"PupilComments", comments.PupilComments);
                command.Parameters.AddWithValue(@"ParentComments", comments.ParentComments);
                command.Parameters.AddWithValue(@"BehaviorNotes", comments.BehaviorNotes);
                command.Parameters.AddWithValue(@"AttendanceNotes", comments.AttendanceNotes);
                command.Parameters.AddWithValue(@"HomeworkNotes", comments.HomeworkNotes);
                command.Parameters.AddWithValue(@"ExtraCurricularNotes", comments.ExtraCurricularNotes);
                command.Parameters.AddWithValue(@"SpecialEducationalNeedsNotes", comments.SpecialEducationalNeedsNotes);
                command.Parameters.AddWithValue(@"SafeguardingNotes", comments.SafeguardingNotes);
                command.Parameters.AddWithValue(@"OtherNotes", comments.OtherNotes);
                command.Parameters.AddWithValue(@"DateAdded", comments.DateAdded);

                await command.ExecuteNonQueryAsync();
                Log.Information("Comments for StudentId {StudentId} added to database.", comments.StudentId);
                return true;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Error adding comments to database.");
                throw;
            }
        }
    }
}
