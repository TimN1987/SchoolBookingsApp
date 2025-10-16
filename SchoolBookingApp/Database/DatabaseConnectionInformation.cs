using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.Database;

/// <summary>
/// Represents the connection information for the database.
/// </summary>
public class DatabaseConnectionInformation
{
    public string ApplicationFolder { get; set; } = string.Empty;
    public string DatabaseFolder { get; set; } = string.Empty;
    public string DatabaseDirectoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationFolder, DatabaseFolder);
    public string DatabaseFileName { get; set; } = string.Empty;
    public string DatabaseFilePath => Path.Combine(DatabaseDirectoryPath, DatabaseFileName);
    public string ConnectionString => $"Data Source={DatabaseFilePath}";
}
