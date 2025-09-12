using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SchoolBookingApp.MVVM.Struct;

namespace SchoolBookingApp.MVVM.Model
{
    public record Student (
        int Id,
        string FirstName,
        string LastName,
        int DateOfBirth,
        string ClassName,
        List<(Parent, string)> Parents,
        StudentDataRecord Data,
        MeetingCommentsRecord Comments
    );
}
