using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Model;

/// <summary>
/// An abstract class for creating people for the database.
/// </summary>
/// <param name="Id">The unique id number from the database.</param>
public abstract record Person(int Id, string FirstName, string LastName);
