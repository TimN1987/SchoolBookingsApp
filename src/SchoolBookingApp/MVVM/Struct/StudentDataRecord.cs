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

        public StudentDataRecord(
        int studentId,
        int math,
        string mathComments,
        int reading,
        string readingComments,
        int writing,
        string writingComments,
        int science,
        int history,
        int geography,
        int mfl,
        int pe,
        int art,
        int music,
        int re,
        int designTechnology,
        int computing
        )
        {
            StudentId = studentId;
            Math = math;
            MathComments = mathComments;
            Reading = reading;
            ReadingComments = readingComments;
            Writing = writing;
            WritingComments = writingComments;
            Science = science;
            History = history;
            Geography = geography;
            MFL = mfl;
            PE = pe;
            Art = art;
            Music = music;
            RE = re;
            DesignTechnology = designTechnology;
            Computing = computing;
        }

        public StudentDataRecord()
            : this(0, 0, string.Empty, 0, string.Empty, 0, string.Empty, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) { }
    }
}
