using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Struct;
using Serilog;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Defines a contract for updating operations in the database with methods for updating parent, student, 
    /// data records, and comments.
    /// </summary>
    public interface IUpdateOperationService
    {
        Task<bool> UpdateParent(int parentId, string? firstName = null, string? lastName = null, List<(int studentId, string relationship)>? studentRelationships = null);
        Task<bool> UpdateStudent(int studentId, string? firstName = null, string? lastName = null, int? dateOfBirth = null, string? className = null);
        Task<bool> UpdateData(StudentDataRecord dataRecord);
        Task<bool> UpdateComments(MeetingCommentsRecord comments);
    }

    /// <summary>
    /// A class for updating records in the Sqlite database. Contains methods for updating parent, student, 
    /// data records, and comments. It uses the <see cref="SqliteConnection"/> to connect to the database and 
    /// perform update operations. The class implements the <see cref="IUpdateOperationService"/> interface,
    /// </summary>
    /// <param name="connection">A Sqlite connection to the database.</param>
    public class UpdateOperationService(SqliteConnection connection) : IUpdateOperationService
    {
        private readonly SqliteConnection _connection = connection
            ?? throw new ArgumentNullException(nameof(connection), "Connection cannot be null.");

        //SQL command strings for updating records in the database.
        private readonly string _updateCommandTemplate = @"
            UPDATE {0}
            SET {1} = {2}
            WHERE {3} = {4};";
        private readonly string _updateParentStudentsRelationship = @"
            INSERT OR REPLACE INTO ParentStudents (
                ParentId,
                StudentId,
                Relationship)
            VALUES (
                @parentId,
                @studentId,
                @relationship);";
        private readonly string _deleteExistingRelationshipCommand = @"
            DELETE FROM ParentStudents WHERE ParentId = @id";

        //Methods

        /// <summary>
        /// Updates the details of a parent in the database. It allows updating the parent's first name, last 
        /// name, and the relationship with students. If any of the parameters are null or empty, they will not 
        /// be updated.
        /// </summary>
        /// <param name="parentId">The id of the parent to be updated.</param>
        /// <param name="firstName">The updated first name to be updated if given.</param>
        /// <param name="lastName">The updated last name to be updated if given.</param>
        /// <param name="studentRelationships">The student relationships to be updated if given.</param>
        /// <returns><c>true</c> if all updates are completed successfully. <c>false</c> if the updates failed.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid parent id is entered.</exception>
        /// <remarks>Validate user inputs to ensure that this method is used for changes to the parent data.</remarks>
        public async Task<bool> UpdateParent(int parentId, string? firstName = null, string? lastName = null, List<(int studentId, string relationship)>? studentRelationships = null)
        {
            if (parentId <= 0)
                throw new ArgumentException("Invalid Parent Id entered.");

            try
            {
                await using var transaction = _connection.BeginTransaction();

                if (!string.IsNullOrEmpty(firstName))
                {
                    using var firstNameCommand = _connection.CreateCommand();
                    firstNameCommand.CommandText = string.Format(_updateCommandTemplate, "Parents", "FirstName", "@firstName", "Id", "@parentId");
                    firstNameCommand.Transaction = transaction;
                    firstNameCommand.Parameters.AddWithValue("@firstName", firstName);
                    firstNameCommand.Parameters.AddWithValue("@parentId", parentId);
                    var firstNameRowsChanged = await firstNameCommand.ExecuteNonQueryAsync();

                    if (firstNameRowsChanged <= 0) return false;
                }

                if (!string.IsNullOrEmpty(lastName))
                {
                    using var lastNameCommand = _connection.CreateCommand();
                    lastNameCommand.CommandText = string.Format(_updateCommandTemplate, "Parents", "LastName", "@lastName", "Id", "@parentId");
                    lastNameCommand.Transaction = transaction;
                    lastNameCommand.Parameters.AddWithValue("@lastName", lastName);
                    lastNameCommand.Parameters.AddWithValue("@parentId", parentId);
                    var lastNameRowsChanged = await lastNameCommand.ExecuteNonQueryAsync();

                    if (lastNameRowsChanged <= 0) return false;
                }

                //Remove previous relationships before ensuring correct list is stored in the database.
                using var deleteRelationshipsCommand = _connection.CreateCommand();
                deleteRelationshipsCommand.CommandText = _deleteExistingRelationshipCommand;
                deleteRelationshipsCommand.Transaction = transaction;
                deleteRelationshipsCommand.Parameters.AddWithValue("@id", parentId);
                await deleteRelationshipsCommand.ExecuteNonQueryAsync();

                if (studentRelationships != null && studentRelationships.Count > 0)
                {
                    foreach (var (studentId, relationship) in studentRelationships)
                    {
                        if (studentId <= 0 || string.IsNullOrEmpty(relationship))
                            throw new ArgumentException("Invalid student relationship data provided.");

                        using var relationshipCommand = _connection.CreateCommand();
                        relationshipCommand.CommandText = _updateParentStudentsRelationship;
                        relationshipCommand.Transaction = transaction;
                        relationshipCommand.Parameters.AddWithValue("@parentId", parentId);
                        relationshipCommand.Parameters.AddWithValue("@studentId", studentId);
                        relationshipCommand.Parameters.AddWithValue("@relationship", relationship);
                        var relationshipRowsChanged = await relationshipCommand.ExecuteNonQueryAsync();

                        if (relationshipRowsChanged <= 0) return false;
                    }
                }

                await transaction.CommitAsync();
                Log.Information("Parent details updated successfully. ParentId: {ParentId}", parentId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error updating parent details.", ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the details of a student in the database. It allows updating of the student's first name, 
        /// last name, date of birth, and class name. If any of the parameters are null or empty, they will not 
        /// be updated.
        /// </summary>
        /// <param name="studentId">The Id of the student to be updated.</param>
        /// <param name="firstName">The updated first name if given.</param>
        /// <param name="lastName">The updated last name if given.</param>
        /// <param name="dateOfBirth">The updated date of birth if given.</param>
        /// <param name="className">The updated class name if given.</param>
        /// <returns><c>true</c> if succesfully updated, else <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown if the student Id is not valid.</exception>
        /// <remarks>Use validation on user inputs to ensure that this method is used for changes to the student data.</remarks>
        public async Task<bool> UpdateStudent(
            int studentId, 
            string? firstName = null, 
            string? lastName = null, 
            int? dateOfBirth = null, 
            string? className = null)
        {
            if (studentId <= 0) 
                throw new ArgumentException("Invalid Student Id entered.");

            try
            {
                await using var transaction = _connection.BeginTransaction();
                
                if (!string.IsNullOrEmpty(firstName))
                {
                    var firstNameCommand = _connection.CreateCommand();
                    firstNameCommand.CommandText = string.Format(
                        _updateCommandTemplate, "Students", "FirstName", "@firstName", "Id", "@studentId");
                    firstNameCommand.Transaction = transaction;
                    firstNameCommand.Parameters.AddWithValue("@firstName", firstName);
                    firstNameCommand.Parameters.AddWithValue("@studentId", studentId);
                    var rowsChanged = await firstNameCommand.ExecuteNonQueryAsync();

                    if (rowsChanged <= 0) 
                        return false;
                }

                if (!string.IsNullOrEmpty(lastName))
                {
                    var lastNameCommand = _connection.CreateCommand();
                    lastNameCommand.CommandText = string.Format(
                        _updateCommandTemplate, "Students", "LastName", "@lastName", "Id", "@studentId");
                    lastNameCommand.Transaction = transaction;
                    lastNameCommand.Parameters.AddWithValue("@lastName", lastName);
                    lastNameCommand.Parameters.AddWithValue("@studentId", studentId);
                    var rowsChanged = await lastNameCommand.ExecuteNonQueryAsync();

                    if (rowsChanged <= 0)
                        return false;
                }

                if (dateOfBirth > 0)
                {
                    var dateOfBirthCommand = _connection.CreateCommand();
                    dateOfBirthCommand.CommandText = string.Format(
                        _updateCommandTemplate, "Students", "DateOfBirth", "@dateOfBirth", "Id", "@studentId");
                    dateOfBirthCommand.Transaction = transaction;
                    dateOfBirthCommand.Parameters.AddWithValue("@dateOfBirth", dateOfBirth);
                    dateOfBirthCommand.Parameters.AddWithValue("@studentId", studentId);
                    var rowsChanged = await dateOfBirthCommand.ExecuteNonQueryAsync();

                    if (rowsChanged <= 0)
                        return false;
                }

                if (!string.IsNullOrEmpty(className))
                {
                    var classCommand = _connection.CreateCommand();
                    classCommand.CommandText = string.Format(
                        _updateCommandTemplate, "Students", "Class", "@className", "Id", "@studentId");
                    classCommand.Transaction = transaction;
                    classCommand.Parameters.AddWithValue("@className", className);
                    classCommand.Parameters.AddWithValue("@studentId", studentId);
                    var rowsChanged = await classCommand.ExecuteNonQueryAsync();

                    if (rowsChanged <= 0)
                        return false;
                }

                await transaction.CommitAsync();
                Log.Information("Student details updated successfully. StudentId: {StudentId}", studentId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error updating student details.", ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the data for a given student. This method allows updating various academic records stored in 
        /// the <c>Data</c> table.
        /// </summary>
        /// <param name="dataRecord">A record of updates for the given student's data.</param>
        /// <returns><c>true</c> if successfully updated, else <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown if the student Id is invalid.</exception>
        public async Task<bool> UpdateData(StudentDataRecord dataRecord)
        {
            if (dataRecord.StudentId < 0)
                throw new ArgumentException("Invalid Student Id entered.");

            var updates = new Dictionary<string, object?>()
            {
                { "Math", dataRecord.Math > 0 ? dataRecord.Math : null },
                { "MathComments", string.IsNullOrEmpty(dataRecord.MathComments) ? null : dataRecord.MathComments },
                { "Reading", dataRecord.Reading > 0 ? dataRecord.Reading : null },
                { "ReadingComments", string.IsNullOrEmpty(dataRecord.ReadingComments) ? null : dataRecord.ReadingComments },
                { "Writing", dataRecord.Writing > 0 ? dataRecord.Writing : null },
                { "WritingComments", string.IsNullOrEmpty(dataRecord.WritingComments) ? null : dataRecord.WritingComments },
                { "Science", dataRecord.Science > 0 ? dataRecord.Science : null },
                { "History", dataRecord.History > 0 ? dataRecord.History : null },
                { "Geography", dataRecord.Geography > 0 ? dataRecord.Geography : null },
                { "MFL", dataRecord.MFL > 0 ? dataRecord.MFL : null },
                { "PE", dataRecord.PE > 0 ? dataRecord.PE : null },
                { "Art", dataRecord.Art > 0 ? dataRecord.Art : null },
                { "Music", dataRecord.Music > 0 ? dataRecord.Music : null },
                { "DesignTechnology", dataRecord.DesignTechnology > 0 ? dataRecord.DesignTechnology : null },
                { "Computing", dataRecord.Computing > 0 ? dataRecord.Computing : null },
                { "RE", dataRecord.RE > 0 ? dataRecord.RE : null },
            };

            try
            {
                using var transaction = _connection.BeginTransaction();

                foreach (var update in updates)
                {
                    if (update.Value == null)
                        continue;
                    if (!await UpdateColumn("Data", update.Key, update.Value, "Id", dataRecord.StudentId, transaction))
                        return false;
                }

                await transaction.CommitAsync();
                Log.Information("Student data record updated successfully for StudentId {StudentId}.", dataRecord.StudentId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error updating student data record.", ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the comments for a given student. This method allows updating various types of comments including 
        /// validation to ensure that the student Id is valid and that the comments are not empty or null.
        /// </summary>
        /// <param name="comments">A record of comments to be updated.</param>
        /// <returns><c>true</c> if the table is successfully updated, else <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown if the student Id is invalid.</exception>
        public async Task<bool> UpdateComments(MeetingCommentsRecord comments)
        {
            if (comments.StudentId <= 0)
                throw new ArgumentException("Invalid Student Id entered.");

            var updates = new Dictionary<string, object?>
            {
                { "GeneralComments", string.IsNullOrEmpty(comments.GeneralComments) ? null : comments.GeneralComments },
                { "PupilComments", string.IsNullOrEmpty(comments.PupilComments) ? null : comments.PupilComments },
                { "ParentComments", string.IsNullOrEmpty(comments.ParentComments) ? null : comments.ParentComments },
                { "BehaviorNotes", string.IsNullOrEmpty(comments.BehaviorNotes) ? null : comments.BehaviorNotes },
                { "AttendanceNotes", string.IsNullOrEmpty(comments.AttendanceNotes) ? null : comments.AttendanceNotes },
                { "HomeworkNotes", string.IsNullOrEmpty(comments.HomeworkNotes) ? null : comments.HomeworkNotes },
                { "ExtraCurricularNotes", string.IsNullOrEmpty(comments.ExtraCurricularNotes) ? null : comments.ExtraCurricularNotes },
                { "SpecialEducationalNeedsNotes", string.IsNullOrEmpty(comments.SpecialEducationalNeedsNotes) ? null : comments.SpecialEducationalNeedsNotes },
                { "SafeguardingNotes", string.IsNullOrEmpty(comments.SafeguardingNotes) ? null : comments.SafeguardingNotes },
                { "OtherNotes", string.IsNullOrEmpty(comments.OtherNotes) ? null : comments.OtherNotes },
                { "DateAdded", comments.DateAdded > 0 ? comments.DateAdded : null }
            };

            try
            {
                using var transaction = _connection.BeginTransaction();

                foreach (var update in updates)
                {
                    if (update.Value == null)
                        continue;
                    if (!await UpdateColumn("Comments", update.Key, update.Value, "StudentId", comments.StudentId, transaction))
                        return false;
                }

                await transaction.CommitAsync();
                Log.Information("Comments updated successfully for student {StudentId}.", comments.StudentId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error updating comments for student {StudentId}.", comments.StudentId, ex);
                return false;
            }
        }

        //Helper methods

        /// <summary>
        /// Updates a specific column in a given table based on the provided key column and value.
        /// </summary>
        /// <param name="table">The table to be updated.</param>
        /// <param name="updateColumn">The column to be updated.</param>
        /// <param name="updateValue">The new value to update.</param>
        /// <param name="keyColumn">The column containing the primary key for the table.</param>
        /// <param name="keyValue">The primary key value for the record to be updated.</param>
        /// <param name="transaction">The <see cref="SqliteTransaction"/> being used to handle the 
        /// updates.</param>
        /// <returns><c>true</c> if the update is successful, else <c>false</c>.</returns>
        private async Task<bool> UpdateColumn(
            string table, 
            string updateColumn, object updateValue, 
            string keyColumn, object keyValue, 
            SqliteTransaction transaction)
        {
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = string.Format(_updateCommandTemplate, table, updateColumn, "@updateValue", keyColumn, "@keyValue");
                command.Transaction = transaction;
                command.Parameters.AddWithValue("@updateValue", updateValue);
                command.Parameters.AddWithValue("@keyValue", keyValue);
                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating column {UpdateColumn} in table {Table}.", updateColumn, table);
                return false;
            }
        }
    }
}
