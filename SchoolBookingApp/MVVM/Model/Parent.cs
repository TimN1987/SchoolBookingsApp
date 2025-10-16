using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Model;

public record Parent(int Id, string FirstName, string LastName, List<(int id, string relastionship)> Children) 
    : Person(Id, FirstName, LastName);
