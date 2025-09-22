using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using SchoolBookingApp.MVVM.Model;
using System.Diagnostics;

namespace SchoolBookingApp.MVVM.Converters
{
    /// <summary>
    /// A class for converting a <see cref="ValueTuple"/> containing a <see cref="Person"/> and a <see langword="string"/> 
    /// into a <see cref="Relationship"/> that can easily display the relationship information using data bindings and 
    /// string formats.
    /// </summary>
    public class ValueTupleToRelationshipConverter : IValueConverter
    {
        /// <summary>
        /// Converts the <see cref="ValueTuple"/> (<see cref="Person"/>, <see langword="string"/>) into a <see 
        /// cref="Relationship"/>.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<(Student person, string relationship)> studentList)
                return studentList
                    .Select(item => new Relationship(item.person, item.relationship))
                    .ToList();

            if (value is List<(Parent person, string relationship)> parentList)
                return parentList
                    .Select(item => new Relationship(item.person, item.relationship))
                    .ToList();

            return Binding.DoNothing;
        }

        /// <summary>
        /// Converts the <see cref="Relationship"/> back into a <see cref="ValueTuple"/> (see <see cref="Person"/>, <see 
        /// langword="string"/>.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<Relationship> relationships)
                return relationships
                    .Select(relationship => (relationship.Person, relationship.RelationshipType))
                    .ToList();
            return Binding.DoNothing;
        }
    }
}
