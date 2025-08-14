using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Represents the result of a search operation, containing details about an individual match. It includes the details 
    /// to be display in a search result list.
    /// </summary>
    public struct SearchResult
    {
        public int Id;
        public string FirstName;
        public string LastName;
        public string Category;
    }
}
