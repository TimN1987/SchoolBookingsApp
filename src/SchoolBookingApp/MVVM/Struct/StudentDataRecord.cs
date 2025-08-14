using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolBookingApp.MVVM.Struct
{
    /// <summary>
    /// Represents a record of student data in the school booking application.
    /// </summary>
    public struct StudentDataRecord
    {
        public int StudentId { get; set; }
        public int Math { get; set; }
        public string MathComments { get; set; }
        public int Reading { get; set; }
        public string ReadingComments { get; set; }
        public int Writing { get; set; }
        public string WritingComments { get; set; }
        public int Science { get; set; }
        public int History { get; set; }
        public int Geography { get; set; }
        public int MFL { get; set; }
        public int PE { get; set; }
        public int Art { get; set; }
        public int Music { get; set; }
        public int RE { get; set; }
        public int DesignTechnology { get; set; }
        public int Computing { get; set; }

    }
}
