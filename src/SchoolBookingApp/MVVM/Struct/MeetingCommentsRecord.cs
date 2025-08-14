using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Struct
{
    /// <summary>  
    /// Represents a record of comments from parents' meetings.  
    /// </summary>  
    public struct MeetingCommentsRecord
    {
        public int StudentId { get; set; }
        public string GeneralComments { get; set; }
        public string PupilComments { get; set; }
        public string ParentComments { get; set; }
        public string BehaviorNotes { get; set; }
        public string AttendanceNotes { get; set; }
        public string HomeworkNotes { get; set; }
        public string ExtraCurricularNotes { get; set; }
        public string SpecialEducationalNeedsNotes { get; set; }
        public string SafeguardingNotes { get; set; }
        public string OtherNotes { get; set; }
        public int DateAdded { get; set; }
    }
}
