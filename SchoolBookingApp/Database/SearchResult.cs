using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.Database;

/// <summary>
/// Represents the result of a search operation, containing details about an individual match. It includes the details 
/// to be display in a search result list.
/// </summary>
public struct SearchResult
{
    public int Id {  get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Category { get; init; }
}
