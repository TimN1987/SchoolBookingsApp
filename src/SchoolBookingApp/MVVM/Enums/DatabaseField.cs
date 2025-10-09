using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Database;

namespace SchoolBookingApp.MVVM.Enums
{
    /// <summary>
    /// Database fields available for search criteria in the <see cref="ReadOperationService.SearchByCriteria"/> method.
    /// </summary>
    public enum DatabaseField
    {
        StudentId,
        FirstName,
        LastName,
        DateOfBirth,
        Class,
        ParentFirstName,
        ParentLastName,
        Math,
        MathComments,
        Reading,
        ReadingComments,
        Writing,
        WritingComments,
        Science,
        History,
        Geography,
        MFL,
        PE,
        Art,
        Music,
        DesignTechnology,
        Computing,
        RE,
        GeneralComments,
        PupilComments,
        ParentComments,
        BehaviorNotes,
        AttendanceNotes,
        HomeworkNotes,
        ExtraCurricularNotes,
        SpecialEducationalNeedsNotes,
        SafeguardingNotes,
        OtherNotes,
        DateAdded,
        Invalid
    }
}
