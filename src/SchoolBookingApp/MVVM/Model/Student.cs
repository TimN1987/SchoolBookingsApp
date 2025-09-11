using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SchoolBookingApp.MVVM.Struct;

namespace SchoolBookingApp.MVVM.Model
{
    public class Student (
        int id,
        string firstName,
        string lastName,
        int dateOfBirth,
        string className,
        List<(Parent, string)> parents,
        StudentDataRecord data,
        MeetingCommentsRecord comments
    )
    {
        public int Id { get; set; } = id;
        public string FirstName { get; set; } = firstName;
        public string LastName { get; set; } = lastName;
        public int DateOfBirth { get; set; } = dateOfBirth;
        public string ClassName { get; set; } = className;
        public List<(Parent, string)> Parents { get; set; } = parents;
        public StudentDataRecord Data { get; set; } = data;
        public MeetingCommentsRecord Comments { get; set; } = comments;
    }
}
