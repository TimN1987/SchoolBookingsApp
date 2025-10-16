using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Model;

/// <summary>
/// A class for storing a parent-student relationships, containing the selected person and the parent relationship to that 
/// student. Used for lists of children, lists of parents or enabling data bindings to display children/parents.
/// </summary>
public record Relationship(Person Person, string RelationshipType);
