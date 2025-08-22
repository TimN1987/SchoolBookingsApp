using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Enums;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Struct;
using Serilog;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Defines a contract for the <see cref="ReadOperationService"/> class. Ensures that methods for retrieving key lists 
    /// of data and performing searches by keywords or user-generated criteria.
    /// </summary>
    public interface IReadOperationService
    {
        Task<List<SearchResult>> GetAllSearchData();
        Task<List<SearchResult>> SearchByKeyword(string keyword, string? tableName = null, string? field = null);
        Task<List<Student>> SearchByCriteria(List<SearchCriteria> criteria);
        Task<Student> GetStudentData(int id);
        Task<Parent> GetParentInformation(int id);
        Task<List<SearchResult>> GetStudentList();
        Task<List<SearchResult>> GetParentList();
        Task<List<string>> ListClasses();
        Task<List<Student>> GetClassList(string className);
    }

    /// <summary>
    /// A class for handling read operations on the database with a focus on student and parent information, data and 
    /// meeting comments. Enables the user to retrieve key lists of information or data as well as providing search 
    /// functionality, both by keyword or by lists of key criteria. Includes helper methods to ensure that these functions 
    /// can be performed straightforwardly.
    /// </summary>
    public class ReadOperationService : IReadOperationService
    {
        private readonly SqliteConnection _connection;
        private readonly HashSet<string> _validTables;
        private readonly HashSet<string> _validSearchFields;
        private readonly Dictionary<string, HashSet<string>> _validFields;
        private readonly Dictionary<DatabaseField, string> _fieldMapping;

        //SQL query strings
        private readonly string _getAllDataQuery;
        private readonly string _searchByKeywordQuery;
        private readonly string _searchByKeywordAdditionalFieldsQuery;
        private readonly string _listClassesQuery;
        private readonly string _searchByCriteriaBaseQuery;
        private readonly string _parentListQuery;

        public ReadOperationService(SqliteConnection connection)
        {
            _connection = connection
                ?? throw new ArgumentNullException(nameof(connection));
            if (_connection.State != ConnectionState.Open)
                throw new InvalidOperationException("The provided connection must be open.");

            _validTables = ["Students", "Parents", "ParentStudents", "Data", "Comments", "All"];
            _validSearchFields = ["FirstName", "LastName", "All"];
            _validFields = new Dictionary<string, HashSet<string>> {
                { "Students", ["Id", "FirstName", "LastName", "DateOfBirth", "Class"] },
                { "Parents", ["Id", "FirstName", "LastName"] },
                { "ParentStudents", ["ParentId", "StudentId", "Relationship"] },
                { "Data", ["Id", "StudentId", "Math", "MathComments", "Reading", "ReadingComments", "Writing", "WritingComments", "Science", "History", "Geography", "MFL", "PE", "Art", "Music", "DesignTechnology", "Computing", "RE"] },
                { "Comments", ["Id", "StudentId", "GeneralComments", "PupilComments", "ParentComments", "BehaviorNotes", "AttendanceNotes", "HomeworkNotes", "ExtraCurricularNotes", "SpecialEducationalNeedsNotes", "SafeguardingNotes", "OtherNotes"] }
            };
            _fieldMapping = new()
            {
                { DatabaseField.StudentId, "s.Id" },
                { DatabaseField.FirstName, "s.FirstName" },
                { DatabaseField.LastName, "s.LastName" },
                { DatabaseField.DateOfBirth, "s.DateOfBirth" },
                { DatabaseField.Class, "s.Class" },
                { DatabaseField.Math, "d.Math" },
                { DatabaseField.MathComments, "d.MathComments" },
                { DatabaseField.Reading, "d.Reading" },
                { DatabaseField.ReadingComments, "d.ReadingComments" },
                { DatabaseField.Writing, "d.Writing" },
                { DatabaseField.WritingComments, "d.WritingComments" },
                { DatabaseField.Science, "d.Science" },
                { DatabaseField.History, "d.History" },
                { DatabaseField.Geography, "d.Geography" },
                { DatabaseField.MFL, "d.MFL" },
                { DatabaseField.PE, "d.PE" },
                { DatabaseField.Art, "d.Art" },
                { DatabaseField.Music, "d.Music" },
                { DatabaseField.DesignTechnology, "d.DesignTechnology" },
                { DatabaseField.Computing, "d.Computing" },
                { DatabaseField.RE, "d.RE" },
                { DatabaseField.GeneralComments, "c.GeneralComments" },
                { DatabaseField.PupilComments, "c.PupilComments" },
                { DatabaseField.ParentComments, "c.ParentComments" },
                { DatabaseField.BehaviorNotes, "c.BehaviorNotes" },
                { DatabaseField.AttendanceNotes, "c.AttendanceNotes" },
                { DatabaseField.HomeworkNotes, "c.HomeworkNotes" },
                { DatabaseField.ExtraCurricularNotes, "c.ExtraCurricularNotes" },
                { DatabaseField.SpecialEducationalNeedsNotes, "c.SpecialEducationalNeedsNotes" },
                { DatabaseField.SafeguardingNotes, "c.SafeguardingNotes" },
                { DatabaseField.OtherNotes, "c.OtherNotes" },
                { DatabaseField.DateAdded, "c.DateAdded" }
            };

            //SQL query strings
            _getAllDataQuery = @"SELECT * FROM {0};";
            _searchByKeywordQuery = @"SELECT * FROM {0} WHERE LOWER({1}) LIKE @Keyword";
            _searchByKeywordAdditionalFieldsQuery = @" OR LOWER({0}) LIKE @Keyword";
            _listClassesQuery = @"SELECT DISTINCT Class FROM Students;";
            _searchByCriteriaBaseQuery = @"
                SELECT 
                    s.Id,
                    s.FirstName, 
                    s.LastName,
                    s.DateOfBirth,
                    s.Class,
                    GROUP_CONCAT(p.Id || ', ' || p.FirstName || ', ' || p.LastName || ', ' || ps.Relationship, ', ') AS ParentNames,
                    d.Math, d.MathComments,
                    d.Reading, d.ReadingComments,
                    d.Writing, d.WritingComments,
                    d.Science, d.History, d.Geography, d.MFL, d.PE, d.Art, d.Music, d.DesignTechnology, d.Computing, d.RE,
                    c.GeneralComments,
                    c.PupilComments,
                    c.ParentComments,
                    c.BehaviorNotes,
                    c.AttendanceNotes,
                    c.HomeworkNotes,
                    c.ExtraCurricularNotes,
                    c.SpecialEducationalNeedsNotes,
                    c.SafeguardingNotes,
                    c.OtherNotes,
                    c.DateAdded
                FROM Students AS s
                LEFT JOIN ParentStudents AS ps ON s.Id = ps.StudentId
                LEFT JOIN Parents AS p ON ps.ParentId = p.Id
                LEFT JOIN Data AS d ON s.Id = d.StudentId
                LEFT JOIN Comments AS c ON s.Id = c.StudentId
                WHERE 1 = 1"; //Base query for searching by criteria, to be appended with conditions.
            _parentListQuery = @"
                SELECT 
                    p.FirstName, 
                    p.LastName, 
                    ps.Relationship,
                    ps.StudentId AS ChildId
                FROM Parents AS p
                LEFT JOIN ParentStudents AS ps
                ON p.Id = ps.ParentId
                WHERE p.Id = @id;";
        }

        //Methods

        /// <summary>
        /// Retrieves all search data from the database, i.e. the names of parents and students, to be displayed in search 
        /// results or for lists of all names in the database.
        /// </summary>
        /// <returns>A list of <see cref="SearchResult"/>s containing the Id, first name and last name for each parent 
        /// abd student in the database.</returns>
        public async Task<List<SearchResult>> GetAllSearchData()
        {
            var results = new List<SearchResult>();

            try
            {
                results.AddRange(await ReadAllNameDataFromTable("Parents"));
                results.AddRange(await ReadAllNameDataFromTable("Students"));

                return results;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in retrieving search data.");
                return []; //Return empty list to avoid UI display partial or broken data.
            }
        }

        /// <summary>
        /// Searches the database for records matching a given keyword in a specified field of a specified table. Allows 
        /// a user to search for records in either the "Students" or "Parents" table, or both, to find records that have a 
        /// first name or last name matching the keyword. If the keyword is empty, returns all search data.
        /// </summary>
        /// <param name="keyword">The whole or partial name to be matched.</param>
        /// <param name="tableName">The table to be searched. A null entry implies a search in Parents and Students.</param>
        /// <param name="field">The field to be searched. A null entry implies a search in FirstName and LastName.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown in the <paramref name="keyword"/> is null.</exception>"
        /// <exception cref="ArgumentException">Thrown if the <paramref name="keyword"/>, <paramref name="field"/> or 
        /// <paramref name="tableName"/> are invalid.</exception>
        public async Task<List<SearchResult>> SearchByKeyword(string keyword, string? tableName = null, string? field = null)
        {
            //Validate keyword
            ArgumentNullException.ThrowIfNull(keyword, nameof(keyword));
            if (string.IsNullOrEmpty(keyword))
                return await GetAllSearchData();
            if (string.IsNullOrWhiteSpace(keyword))
                return [];

            //Validate table name
            tableName ??= "All";
            if (string.IsNullOrWhiteSpace(tableName) || !_validTables.Contains(tableName))
                throw new ArgumentException($"Invalid table name: {tableName}. Valid tables are: {string.Join(", ", _validTables)}");

            //Validate field
            field ??= "All";
            if (string.IsNullOrWhiteSpace(field) || !_validSearchFields.Contains(field))
                throw new ArgumentException($"Invalid field: {field}. Valid fields are: {string.Join(", ", _validSearchFields)}");

            var results = new List<SearchResult>();

            try
            {
                if (tableName != "Parents")
                {
                    using var command = _connection.CreateCommand();
                    command.CommandText = ConstructSearchCommandText("Students", field);
                    command.Parameters.AddWithValue("@Keyword", $"{keyword.ToLower()}%");
                    await using var reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            results.Add(new SearchResult()
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Category = "Students"
                            });
                        }
                    }
                }

                if (tableName != "Students")
                {
                    using var command = _connection.CreateCommand();
                    command.CommandText = ConstructSearchCommandText("Parents", field);
                    command.Parameters.AddWithValue("@Keyword", $"{keyword.ToLower()}%");
                    await using var reader = await command.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            results.Add(new SearchResult()
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Category = "Parents"
                            });
                        }
                    }
                }

                return results.Distinct().ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error searching for data by keyword: {keyword}.");
                return [];
            }
        }

        /// <summary>
        /// Searches the database for students that match all of a list of criteria. Allows a user to retrieve all the 
        /// information stored about students based on a range of criteria across all the information tables in the 
        /// database (for example, greater than a value, equals a value or not between two values).
        /// </summary>
        /// <param name="criteria">A list of <see cref="SearchCriteria"/> containing the field to search on, the type of 
        /// comparison and the search parameters.</param>
        /// <returns>A list of <see cref="Student"/> records that match the search criteria. If there are no matching 
        /// students, an empty list is returned. If there are no criteria, all students are included in the results.</returns>
        public async Task<List<Student>> SearchByCriteria(List<SearchCriteria> criteria)
        {
            //Validate criteria
            if (criteria == null)
                return [];

            var commandText = GenerateSearchQuery(criteria);

            try
            {
                using var command = GenerateSearchCommand(commandText, criteria);

                await using SqliteDataReader reader = await command.ExecuteReaderAsync();

                List<Student> results = ReadStudentDataFromSearch(reader);

                Log.Information($"Search by criteria returned {results.Count} results.");
                return results;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching by criteria.");
                return [];
            }
        }

        /// <summary>
        /// Retrieves all of the data stored for a specific student based on their Id number. This includes names, parent 
        /// information, academic data and meeting records.
        /// </summary>
        /// <param name="id">The student id for the given student.</param>
        /// <returns>A <see cref="Student"/> record containing all the data for given student.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid student id (<= 0) is entered.</exception>
        public async Task<Student> GetStudentData(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid student Id.");

            var criteria = new List<SearchCriteria>
            {
                new (
                    DatabaseField.StudentId, SQLOperator.Equals, [ id ]
                    )
            };

            return (await SearchByCriteria(criteria)).FirstOrDefault()
                ?? new Student(id, "No student found", "No student found", 0, "", [], new(), new());
        }

        /// <summary>
        /// Retrieves all information for a given parent from their parent id, including first name, last name and a list 
        /// of children with their relationship to the parent. Allows a user to view all the information stored about a 
        /// given parent.
        /// </summary>
        /// <param name="id">The parent id for the given parent.</param>
        /// <returns>A <see cref="Parent"/> record containing all information for the given parent.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid id (<= 0) is entered.</exception>
        public async Task<Parent> GetParentInformation(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid parent Id.");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = _parentListQuery;
                command.Parameters.AddWithValue("@id", id);
                await using var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    var firstName = string.Empty;
                    var lastName = string.Empty;
                    var children = new List<(int id, string relationship)>();

                    while (reader.Read())
                    {
                        if (string.IsNullOrEmpty(firstName))
                            firstName = GetSafeString(reader, "FirstName");
                        if (string.IsNullOrEmpty(lastName))
                            lastName = GetSafeString(reader, "LastName");
                        var relationship = GetSafeString(reader, "Relationship");
                        var childId = GetSafeInt(reader, "ChildId");
                        children.Add((childId, relationship));
                    }

                    return new Parent(
                        id,
                        firstName,
                        lastName,
                        children
                        );
                }
                else throw new InvalidOperationException("Could not find parent information.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving parent information.");
                return new Parent(
                    id,
                    "No name found.",
                    "No name found.",
                    []
                    );
            }
        }

        /// <summary>
        /// Retrieves a list of <see cref="SearchResult"/>s for all students in the database, containing student id, first 
        /// name and last name. Allows the user to see a list of all students for easy selection and searching.
        /// </summary>
        /// <returns>A list of <see cref="SearchResult"/> records for all the students, containing id, first name and last 
        /// name.</returns>
        public async Task<List<SearchResult>> GetStudentList()
        {
            return await ReadAllNameDataFromTable("Students");
        }

        /// <summary>
        /// Retrieves a list of <see cref="SearchResult"/>s for all parents in the database, containing parent id, first 
        /// name and last name. Allows the user to see a list of all parents for easy selection and searching.
        /// </summary>
        /// <returns>A list of <see cref="SearchResult"/> records for all the students, containing id, first name and last 
        /// name.</returns>
        public async Task<List<SearchResult>> GetParentList()
        {
            return await ReadAllNameDataFromTable("Parents");
        }

        /// <summary>
        /// Returns a list of all the classes included in the <c>Students</c> table in the database. Allows the user to 
        /// quickly see all current classes for easy class selection or searching by class.
        /// </summary>
        /// <returns>A list of strings containing the current class names in the <c>Students</c> table.</returns>
        public async Task<List<string>> ListClasses()
        {
            var results = new List<string>();
            
            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = _listClassesQuery;

                await using var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        results.Add(reader.GetString(0));
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error listing classes.");
                return [];
            }
        }

        /// <summary>
        /// Returns a list of <see cref="Student"/> records for each student in a given class. Allows the user to access 
        /// information about all students in the same class.
        /// </summary>
        /// <param name="className">The name of the class list that should be returned.</param>
        /// <returns>A list of <see cref="Student"/> records for the given class name.</returns>
        /// <exception cref="ArgumentException">Thrown if the class name is null, empty or whitespace.</exception>
        public async Task<List<Student>> GetClassList(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentException("Class name cannot be null or empty.", nameof(className));

            var criteria = new List<SearchCriteria>
            {
                new (
                    DatabaseField.Class, SQLOperator.Equals, [ className ]
                    )
            };

            return await SearchByCriteria(criteria);
        }

        //Helper methods

        /// <summary>
        /// Reads all of the names from a given name and returns these in a list with the record Id numbers. Used to 
        /// create lists of parents or students to display.
        /// </summary>
        /// <param name="tableName">The name of the table containing the desired data.</param>
        /// <returns>A list of <see cref="SearchResult"/>s containing the Id, first name and last name for each person in 
        /// the given table.</returns>
        private async Task<List<SearchResult>> ReadAllNameDataFromTable(string tableName)
        {
            var results = new List<SearchResult>();

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = string.Format(_getAllDataQuery, tableName);
                await using var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        results.Add(new SearchResult()
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Category = tableName
                        });
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in reading data from the database.");
                return [];
            }
        }

        /// <summary>
        /// Constructs the SQL command text for searching a specific field in a table. Constructs searches on all fields 
        /// if the field is "All". Allows for an unknown search keyword to be added as a parameter later.
        /// </summary>
        /// <param name="tableName">The name of the table to search.</param>
        /// <param name="field">The name of the field to be searched. Allows "All" if all fields should be searched.</param>
        /// <returns>A string representing the SQL command text to execute the desired search.</returns>
        private string ConstructSearchCommandText(string tableName, string field)
        {
            var commandText = new StringBuilder();

            if (field == "All")
            {
                commandText.AppendFormat(_searchByKeywordQuery, tableName, "FirstName");
                commandText.AppendFormat(_searchByKeywordAdditionalFieldsQuery, "LastName");
            }
            else
            {
                commandText.AppendFormat(_searchByKeywordQuery, tableName, field);
            }
            commandText.Append(';');

            return commandText.ToString();
        }

        /// <summary>
        /// Parses a string containing parent details into a list of <see cref="Parent"/> objects. The string is expected 
        /// to be formatted as a comma-separated list of parent details, where each parent's details are represented by 
        /// an id, first name, last name, and relationship to the child.
        /// </summary>
        /// <param name="parentList">A string containing the details for the adults with a relationship to a given child.</param>
        /// <param name="childId">The student Id of the child with a relationship to the list of adults.</param>
        /// <returns>A list of <see cref="Parent"/> objects for a given child with their relationship.</returns>
        /// <remarks>This is used to parse details from the <see cref="ReadOperationService.SearchByCriteria"/> method.</remarks>
        private static List<(Parent, string)> ParseParentDetails(string parentList, int childId)
        {
            var parents = new List<(Parent, string)>();

            if (string.IsNullOrEmpty(parentList))
                return parents;

            var parentDetails = parentList.Split(", ");
            int i = 0;

            while (i < parentDetails.Length)
            {
                var nextParent = new Parent(
                    int.Parse(parentDetails[i]),
                    parentDetails[i + 1],
                    parentDetails[i + 2],
                    [ (childId, parentDetails[i + 3]) ]
                );

                parents.Add((nextParent, parentDetails[i + 3]));
                i += 4; // Move to the next parent (4 fields per parent).
            }

            return parents;
        }

        /// <summary>
        /// Generates a SQL query string based on the provided search criteria. The criteria are expected to be a list of 
        /// <see cref="SearchCriteria"/> objects, each containing a field, an operator, and parameters for the query.
        /// </summary>
        /// <param name="criteria">A list of <see cref="SearchCriteria"/> object containing the different criteria for 
        /// the desired search.</param>
        /// <returns>A string containing a SQL query to search across all data with the desired criteria.</returns>
        private string GenerateSearchQuery(List<SearchCriteria> criteria)
        {
            var query = new StringBuilder(_searchByCriteriaBaseQuery);

            int queryLine = 0;

            foreach (var criterion in criteria)
            {
                string additionalQuery = GenerateCriteriaQuery(criterion.Field, criterion.Operator, queryLine.ToString());

                if (!string.IsNullOrEmpty(additionalQuery))
                    query.Append(additionalQuery);

                queryLine++;
            }

            query.Append(" GROUP BY s.Id;");

            return query.ToString();
        }

        /// <summary>
        /// Generates a SQL query string for a specific field and operator, appending it to the base query.
        /// </summary>
        /// <param name="field">The name of the database field to be searched.</param>
        /// <param name="op">The operator required for the desired search.</param>
        /// <param name="queryLine">The number of the query to ensure accurate parameter assignment.</param>
        /// <returns>A string containing part of a SQL query, used to add criteria to a search.</returns>
        private string GenerateCriteriaQuery(DatabaseField field, SQLOperator op, string queryLine)
        {
            if (!_fieldMapping.TryGetValue(field, out var column))
                return string.Empty;
            
            var query = new StringBuilder();
            if (op == SQLOperator.Like || op == SQLOperator.NotLike)
            {
                query.Append($" AND LOWER({column}) ");
            }
            else
            {
                query.Append($" AND {column} ");
            }

            string parameter = $"@value{queryLine}", minParameter = $"@minValue{queryLine}", maxParameter = $"@maxValue{queryLine}";

            switch (op)
            {
                case SQLOperator.Equals:
                    query.Append($"= {parameter}");
                    break;
                case SQLOperator.NotEquals:
                    query.Append($"<> {parameter}");
                    break;
                case SQLOperator.GreaterThan:
                    query.Append($"> {parameter}");
                    break;
                case SQLOperator.LessThan:
                    query.Append($"< {parameter}");
                    break;
                case SQLOperator.GreaterThanOrEqual:
                    query.Append($">= {parameter}");
                    break;
                case SQLOperator.LessThanOrEqual:
                    query.Append($"<= {parameter}");
                    break;
                case SQLOperator.Like:
                    query.Append($"LIKE LOWER({parameter})");
                    break;
                case SQLOperator.NotLike:
                    query.Append($"NOT LIKE LOWER({parameter})");
                    break;
                case SQLOperator.Between:
                    query.Append($"BETWEEN {minParameter} AND {maxParameter}");
                    break;
                case SQLOperator.NotBetween:
                    query.Append($"NOT BETWEEN {minParameter} AND {maxParameter}");
                    break;
                default:
                    Log.Error($"Invalid SQL operator: {op}");
                    return string.Empty;
            }

            return query.ToString();
        }

        /// <summary>
        /// Generates a SQL command for searching the database based on the provided command text and criteria.
        /// </summary>
        /// <param name="commandText">The SQL query for searching all data with the given criteria.</param>
        /// <param name="criteria">A list of <see cref="SearchCriteria"/> containing the desired parameters for the 
        /// <see cref="SqliteCommand"/>.</param>
        /// <returns>A <see cref="SqliteCommand"/> to search the database with the given criteria.</returns>
        /// <exception cref="ArgumentException">Thrown if there are not enough parameters for a BETWEEN operator, in 
        /// which case an error would occur when trying to add a non-existant parameter.</exception>
        private SqliteCommand GenerateSearchCommand(string commandText, List<SearchCriteria> criteria)
        {
            var command = _connection.CreateCommand();
            command.CommandText = commandText;

            var queryLine = 0;
            foreach (var criterion in criteria)
            {
                if (criterion.Operator == SQLOperator.Between || criterion.Operator == SQLOperator.NotBetween)
                {
                    if (criterion.Parameters.Length != 2)
                    {
                        Log.Error($"Invalid number of parameters for BETWEEN operator: {criterion.Parameters.Length}. Expected 2.");
                        throw new ArgumentException("BETWEEN operator requires exactly two parameters.");
                    }

                    command.Parameters.AddWithValue($"@minValue{queryLine}", criterion.Parameters[0]);
                    command.Parameters.AddWithValue($"@maxValue{queryLine}", criterion.Parameters[1]);
                }
                else if (criterion.Operator == SQLOperator.Like || criterion.Operator == SQLOperator.NotLike)
                {
                    command.Parameters.AddWithValue($"@value{queryLine}", criterion.Parameters[0].ToString() + '%');
                }
                else
                {
                    command.Parameters.AddWithValue($"@value{queryLine}", criterion.Parameters[0]);
                }

                queryLine++;
            }
            
            return command;
        }

        /// <summary>
        /// Reads student data from a <see cref="SqliteDataReader"/> and returns a list of <see cref="Student"/> objects.
        /// </summary>
        /// <param name="reader">A <see cref="SqliteDataReader"/> generated within the <see cref="SearchByCriteria"/> 
        /// method to read data from the database with given criteria.</param>
        /// <returns>A list of <see cref="Student"/> objects containing the search results.</returns>
        private static List<Student> ReadStudentDataFromSearch(SqliteDataReader reader)
        {
            var results = new List<Student>();
            var count = 0;
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var student = new Student(
                        Id : GetSafeInt(reader, "Id"),
                        FirstName: GetSafeString(reader, "FirstName"),
                        LastName: GetSafeString(reader, "LastName"),
                        DateOfBirth: GetSafeInt(reader, "DateOfBirth"),
                        Class: GetSafeString(reader, "Class"),
                        ParseParentDetails(GetSafeString(reader, "ParentNames"), GetSafeInt(reader, "Id")),
                        new StudentDataRecord()
                        {
                            StudentId = GetSafeInt(reader, "Id"),
                            Math = GetSafeInt(reader, "Math"),
                            MathComments = GetSafeString(reader, "MathComments"),
                            Reading = GetSafeInt(reader, "Reading"),
                            ReadingComments = GetSafeString(reader, "ReadingComments"),
                            Writing = GetSafeInt(reader, "Writing"),
                            WritingComments = GetSafeString(reader, "WritingComments"),
                            Science = GetSafeInt(reader, "Science"),
                            History = GetSafeInt(reader, "History"),
                            Geography = GetSafeInt(reader, "Geography"),
                            MFL = GetSafeInt(reader, "MFL"),
                            PE = GetSafeInt(reader, "PE"),
                            Art = GetSafeInt(reader, "Art"),
                            Music = GetSafeInt(reader, "Music"),
                            DesignTechnology = GetSafeInt(reader, "DesignTechnology"),
                            Computing = GetSafeInt(reader, "Computing"),
                            RE = GetSafeInt(reader, "RE")
                        },
                        new MeetingCommentsRecord()
                        {
                            StudentId = GetSafeInt(reader, "Id"),
                            GeneralComments = GetSafeString(reader, "GeneralComments"),
                            PupilComments = GetSafeString(reader, "PupilComments"),
                            ParentComments = GetSafeString(reader, "ParentComments"),
                            BehaviorNotes = GetSafeString(reader, "BehaviorNotes"),
                            AttendanceNotes = GetSafeString(reader, "AttendanceNotes"),
                            HomeworkNotes = GetSafeString(reader, "HomeworkNotes"),
                            ExtraCurricularNotes = GetSafeString(reader, "ExtraCurricularNotes"),
                            SpecialEducationalNeedsNotes = GetSafeString(reader, "SpecialEducationalNeedsNotes"),
                            SafeguardingNotes = GetSafeString(reader, "SafeguardingNotes"),
                            OtherNotes = GetSafeString(reader, "OtherNotes")
                        }
                        );

                    results.Add(student); count++;
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieves a string value from a <see cref="SqliteDataReader"/> for a given column name, returning an empty 
        /// string if the value is null or empty.
        /// </summary>
        /// <param name="reader">The <see cref="SqliteDataReader"/> used to read the search data.</param>
        /// <param name="columnName">The field name for the column containing the desired data.</param>
        /// <returns>A string containing the desired data.</returns>
        private static string GetSafeString(SqliteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) ? reader.GetString(ordinal) : string.Empty;
        }

        /// <summary>
        /// Retrieves an integer value from a <see cref="SqliteDataReader"/> for a given column name, returning 0 if the 
        /// value is null or empty.
        /// </summary>
        /// <param name="reader">The <see cref="SqliteDataReader"/> used to read the search data.</param>
        /// <param name="columnName">The field name for the column containing the desired data.</param>
        /// <returns>The integer value of the desired data.</returns>
        private static int GetSafeInt(SqliteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) ? reader.GetInt32(ordinal) : 0;
        }
    }
}