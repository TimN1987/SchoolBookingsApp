using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Model
{
    public record Parent
    {
        public int Id { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public List<(int id, string relationship)> Children { get; init; }
        

        public Parent(int id, string firstName, string lastName, List<(int id, string relationship)> children)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Children = children;
        }
    }
}
