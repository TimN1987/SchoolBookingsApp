using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.Enums;

/// <summary>
/// Used for search queries with multiple criteria.
/// </summary>
public enum SQLOperator
{
    Equals,
    NotEquals,
    Like,
    NotLike,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    In,
    NotIn,
    Between,
    NotBetween,
    IsNull,
    IsNotNull,
    Invalid
}
