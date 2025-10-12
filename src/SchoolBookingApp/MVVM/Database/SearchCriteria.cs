using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Enums;

namespace SchoolBookingApp.MVVM.Database
{
    /// <summary>
    /// Used to store search criteria for the <see cref="ReadOperationService.SearchByCriteria"/> method.
    /// </summary>
    /// <param name="Field">The database field to be queried.</param>
    /// <param name="Operator">The type of query to execute.</param>
    /// <param name="Parameters">The parameter(s) for the query.</param>
    public record SearchCriteria(DatabaseField Field, SQLOperator Operator, object[] Parameters)
    {
        public string ParametersString => string.Join(" and ", Parameters ?? Array.Empty<object>());
    }
}
