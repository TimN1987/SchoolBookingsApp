using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Data.Sqlite;
using Serilog;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Defines a contract for initializing a database using an asynchronous task.
    /// </summary>
    public interface IDatabaseInitializer
    {
        /// <summary>
        /// Initializes the database by creating necessary folders and tables.
        /// </summary>
        /// <returns>True if the initialization was successful, otherwise false.</returns>
        Task<bool> InitializeDatabaseAsync();
    }
    
    /// <summary>
    /// A class for initializing an Sqlite database. Contains methods for checking that the directory exists, 
    /// that the database file exists and that the appropriate tables exist.
    /// </summary>
    /// <param name="connectionInformation">A struct containing the database file and directory names as 
    /// well as the connection string.</param>
    public class DatabaseInitializer(SqliteConnection connection, DatabaseConnectionInformation connectionInformation) : IDatabaseInitializer
    {   
        //Constants
        private const int BackoffTime = 200;

        //Table creation strings
        private readonly string _createParentsTable = @"
                CREATE TABLE IF NOT EXISTS Parents (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL
                );";
        private readonly string _createStudentsTable = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    DateOfBirth INTEGER NOT NULL,
                    Class TEXT NOT NULL
                );";
        private readonly string _createParentStudentsTable = @"
                CREATE TABLE IF NOT EXISTS ParentStudents (
                    ParentId INTEGER NOT NULL,
                    StudentId INTEGER NOT NULL,
                    Relationship TEXT NOT NULL,
                    PRIMARY KEY (ParentId, StudentId),
                    FOREIGN KEY (ParentId) REFERENCES Parents(Id) ON DELETE CASCADE,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE
                );";
        private readonly string _createDataTable = @"
                CREATE TABLE IF NOT EXISTS Data (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    Math INTEGER NOT NULL,
                    MathComments TEXT,
                    Reading INTEGER NOT NULL,
                    ReadingComments TEXT,
                    Writing INTEGER NOT NULL,
                    WritingComments TEXT,
                    Science INTEGER NOT NULL,
                    History INTEGER NOT NULL,
                    Geography INTEGER NOT NULL,
                    MFL INTEGER NOT NULL,
                    PE INTEGER NOT NULL,
                    Art INTEGER NOT NULL,
                    Music INTEGER NOT NULL,
                    DesignTechnology INTEGER NOT NULL,
                    Computing INTEGER NOT NULL,
                    RE INTEGER NOT NULL,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE
                );";
        private readonly string _createCommentsTable = @"
                CREATE TABLE IF NOT EXISTS Comments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    GeneralComments TEXT NOT NULL,
                    PupilComments TEXT NOT NULL,
                    ParentComments TEXT NOT NULL,
                    BehaviorNotes TEXT,
                    AttendanceNotes TEXT,
                    HomeworkNotes TEXT,
                    ExtraCurricularNotes TEXT,
                    SpecialEducationalNeedsNotes TEXT,
                    SafeguardingNotes TEXT,
                    OtherNotes TEXT,
                    DateAdded INTEGER NOT NULL,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE
                );";
        private readonly string _createBookingsTable = @"
                    CREATE TABLE IF NOT EXISTS Bookings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StudentId INTEGER NOT NULL,
                        BookingDate INTEGER NOT NULL,
                        TimeSlot INTEGER NOT NULL
                        FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE
                    );";   

        //Connection information
        private readonly SqliteConnection _connection = connection
            ?? throw new ArgumentNullException("Connection cannot be null.");
        private readonly string _databaseDirectoryPath = connectionInformation.DatabaseDirectoryPath
            ?? throw new ArgumentNullException("Connection information cannot be null.");
        private readonly string _databaseFilePath = connectionInformation.DatabaseFilePath
            ?? throw new ArgumentNullException("Connection information cannot be null.");

        //Public methods
        /// <summary>
        /// Initializes the database by ensuring the necessary folders and tables exist. Runs asynchronously 
        /// to avoid blocking the UI thread with up to three retries with exponential backoff.
        /// </summary>
        /// <returns>True if the database is correctly initialized. False if the initialization failed.</returns>
        public async Task<bool> InitializeDatabaseAsync()
        {
            if (!EnsureDirectoryExists())
                return false;

            if (!await EnsureDatabaseExistsAsync())
                return false;

            for (int i = 0; i < 3; i++)
            {
                if (await EnsureTablesExistAsync())
                    return true;
                else if (i == 2)
                    //Break if the third attempt fails.
                    break;
                else
                    //Delay before next attempt.
                    await Task.Delay((int)Math.Pow(2, i) * BackoffTime);
            }

            return false;
        }

        //Private helper methods
        /// <summary>
        /// Creates an application folder, containing a database folder, if one doesn't exist already.
        /// </summary>
        /// <returns>True if the folder exists. False if it cannot be created.</returns>
        private bool EnsureDirectoryExists()
        {
            if (Directory.Exists(_databaseDirectoryPath))
                return true;
            Directory.CreateDirectory(_databaseDirectoryPath);
            return Directory.Exists(_databaseDirectoryPath);
        }
        /// <summary>
        /// Checks that the database file exists. Creates it if not.
        /// </summary>
        /// <returns>True if the database exists or has been created. False if it cannot be created.</returns>
        private async Task<bool> EnsureDatabaseExistsAsync()
        {
            if (File.Exists(_databaseFilePath))
                return true;

            try
            {
                //Ensure that the database support foreign keys.
                var pragma = _connection.CreateCommand();
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                await pragma.ExecuteNonQueryAsync();

            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Error in creating database.");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Asynchronously creates the parents, students, data and comments tables in the database if they do 
        /// not already exist.
        /// </summary>
        /// <returns>True if the tables exist or have been created. False if the tables are not successfully 
        /// created.</returns>
        private async Task<bool> EnsureTablesExistAsync()
        {
            try
            {
                await using var transaction = _connection.BeginTransaction();

                var parentsCommand = _connection.CreateCommand();
                parentsCommand.CommandText = _createParentsTable;
                parentsCommand.Transaction = transaction;

                var studentsCommand = _connection.CreateCommand();
                studentsCommand.CommandText = _createStudentsTable;
                studentsCommand.Transaction = transaction;

                var parentStudentsCommand = _connection.CreateCommand();
                parentStudentsCommand.CommandText = _createParentStudentsTable;
                parentStudentsCommand.Transaction = transaction;

                var dataCommand = _connection.CreateCommand();
                dataCommand.CommandText = _createDataTable;
                dataCommand.Transaction = transaction;

                var commentsCommand = _connection.CreateCommand();
                commentsCommand.CommandText = _createCommentsTable;
                commentsCommand.Transaction = transaction;

                var bookingsCommand = _connection.CreateCommand();
                bookingsCommand.CommandText = _createBookingsTable;
                bookingsCommand.Transaction = transaction;

                await parentsCommand.ExecuteNonQueryAsync();
                await studentsCommand.ExecuteNonQueryAsync();
                await parentStudentsCommand.ExecuteNonQueryAsync();
                await dataCommand.ExecuteNonQueryAsync();
                await commentsCommand.ExecuteNonQueryAsync();
                await bookingsCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Error in creating database tables.");
                return false;
            }
        }
    }
}
