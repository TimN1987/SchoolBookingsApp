using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SchoolBookingApp.MVVM.Model;
using Serilog;

namespace SchoolBookingApp.MVVM.Database
{
    public interface IReadOperationService
    {
        Task<List<SearchResult>> GetAllSearchData();
        Task<List<SearchResult>> SearchByKeyword(string keyword, string? tableName = null, string? field = null);
        Task<List<int>> SearchByCriteria(List<(string, object)> criteria, string? tableName = null);
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
        private readonly string _searchByCriteriaAdditionalQuery;

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
            _searchByCriteriaBaseQuery = @"SELECT * FROM {0} WHERE 1 = 1"; //Base query for searching by criteria, to be appended with conditions.
            _searchByCriteriaAdditionalQuery = @" AND {0} = @{0}"; //Additional query to append to the base query for each criteria.
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

        public async Task<List<int>> SearchByCriteria(List<(string, object)> criteria, string tableName)
        {
            //Validate criteria
            if (criteria == null || criteria.Count == 0)
                return [];

            //Validate table name
            if (string.IsNullOrWhiteSpace(tableName) || !_validTables.Contains(tableName))
                throw new ArgumentException($"Invalid table name: {tableName}. Valid tables are: {string.Join(", ", _validTables)}");

            var results = new List<int>();
            var commandText = new StringBuilder(string.Format(_searchByCriteriaBaseQuery, tableName));

            foreach (var (field, _) in criteria)
            {
                //Validate field and skip if invalid.
                if (string.IsNullOrWhiteSpace(field) || !_validFields[tableName].Contains(field))
                    continue;

                //Append the field condition to the command text.
                commandText.AppendFormat(_searchByCriteriaAdditionalQuery, field);
            }

            commandText.Append(';');

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = commandText.ToString();

                foreach (var (field, value) in criteria)
                {
                    //Skip invalid fields as before to ensure the correct parameters are added.
                    if (string.IsNullOrWhiteSpace(field) || !_validFields[tableName].Contains(field))
                        continue;

                    //Add the parameter to the command.
                    command.Parameters.AddWithValue($"@{field}", value);
                }

                await using var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        //Assuming the first column is the Id.
                        results.Add(reader.GetInt32(0));
                    }
                }

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
    }
}
