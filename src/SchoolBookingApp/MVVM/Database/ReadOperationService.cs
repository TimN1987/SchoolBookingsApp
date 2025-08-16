using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Struct;
using Serilog;

namespace SchoolBookingApp.MVVM.Database
{
    public interface IReadOperationService
    {
        Task<List<SearchResult>> GetAllSearchData();
        Task<List<SearchResult>> SearchByKeyword(string keyword, string? tableName = null, string? field = null);
        Task<List<Student>> SearchByCriteria(List<(string, object[])> criteria);
        Task<Student> GetStudentData(int Id);
        Task<Parent> GetParentInformation(int Id);
        Task<List<SearchResult>> GetStudentList();
        Task<List<SearchResult>> GetParentList();
        Task<List<string>> GetListOfClasses();
        Task<List<Student>> GetClassList(string className);
    }
    
    public class ReadOperationService
    {
        private readonly SqliteConnection _connection;
        private readonly HashSet<string> _validTables;
        private readonly HashSet<string> _validSearchFields;
        private readonly Dictionary<string, HashSet<string>> _validFields;

        //SQL query strings
        private readonly string _getAllDataQuery;
        private readonly string _searchByKeywordQuery;
        private readonly string _searchByKeywordAdditionalFieldsQuery;
        private readonly string _searchByCriteriaBaseQuery;

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

            //SQL query strings
            _getAllDataQuery = @"SELECT * FROM {0};";
            _searchByKeywordQuery = @"SELECT * FROM {0} WHERE LOWER({1}) LIKE @Keyword";
            _searchByKeywordAdditionalFieldsQuery = @" OR LOWER({0}) LIKE @Keyword";
            _searchByCriteriaBaseQuery = @"
                SELECT 
                    s.Id,
                    s.FirstName, 
                    s.LastName,
                    s.DateOfBirth,
                    s.Class,
                    GROUP_CONCAT(p.FirstName || ', ' || p.LastName || ', ' || ps.Relationship, ', ') AS ParentNames,
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
                    c.OtherNotes
                FROM Students AS s
                JOIN ParentStudents AS ps ON s.Id = ps.StudentId
                JOIN Parents AS p ON ps.ParentId = p.Id
                JOIN Data AS d ON s.Id = d.StudentId
                JOIN Comments AS c ON s.Id = c.StudentId
                WHERE 1 = 1"; //Base query for searching by criteria, to be appended with conditions.
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


        // Critera contain SQL query strings and their corresponding values. e.g. AND Age BETWEEN @AgeMin AND @AgeMax
        public async Task<List<Student>> SearchByCriteria(List<(string, object[])> criteria)
        {
            //Validate criteria
            if (criteria == null || criteria.Count == 0)
                return [];

            var results = new List<Student>();
            var commandText = new StringBuilder(_searchByCriteriaBaseQuery);

            foreach (var (query, _) in criteria)
            {
                commandText.Append(query);
            }

            commandText.Append(';');

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = commandText.ToString();

                foreach (var (query, value) in criteria)
                {
                    var parameters = Regex.Matches(query, @"@[a-zA-z]+")
                                        .Select(match => match.Value.ToString())
                                        .ToList();

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        command.Parameters.AddWithValue(parameters[i], value[i]);
                    }
                }

                await using var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var student = new Student(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetDateTime(3),
                            reader.GetString(4),
                            ParseParentDetails(reader.GetString(5), reader.GetInt32(0)),
                            new StudentDataRecord() {
                                StudentId = reader.GetInt32(0),
                                Math = reader.GetInt32(6),
                                MathComments = reader.GetString(7),
                                Reading = reader.GetInt32(8),
                                ReadingComments = reader.GetString(9),
                                Writing = reader.GetInt32(10),
                                WritingComments = reader.GetString(11),
                                Science = reader.GetInt32(12),
                                History = reader.GetInt32(13),
                                Geography = reader.GetInt32(14),
                                MFL = reader.GetInt32(15),
                                PE = reader.GetInt32(16),
                                Art = reader.GetInt32(17),
                                Music = reader.GetInt32(18),
                                DesignTechnology = reader.GetInt32(19),
                                Computing = reader.GetInt32(20),
                                RE = reader.GetInt32(21)
                            },
                            new MeetingCommentsRecord() {
                                StudentId = reader.GetInt32(0),
                                GeneralComments = reader.GetString(22),
                                PupilComments = reader.GetString(23),
                                ParentComments = reader.GetString(24),
                                BehaviorNotes = reader.GetString(25),
                                AttendanceNotes = reader.GetString(26),
                                HomeworkNotes = reader.GetString(27),
                                ExtraCurricularNotes = reader.GetString(28),
                                SpecialEducationalNeedsNotes = reader.GetString(29),
                                SafeguardingNotes = reader.GetString(30),
                                OtherNotes = reader.GetString(31)
                            }
                            );

                        results.Add(student);
                    }
                }

                Log.Information($"Search by criteria returned {results.Count} results.");
                return results;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching by criteria.");
                return [];
            }
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
        private List<(Parent, string)> ParseParentDetails(string parentList, int childId)
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
                    new List<(int id, string relationship)> { (childId, parentDetails[i + 3]) }
                );

                parents.Add((nextParent, parentDetails[i + 3]));
                i += 4; // Move to the next parent (4 fields per parent).
            }

            return parents;
        }
    }
}
