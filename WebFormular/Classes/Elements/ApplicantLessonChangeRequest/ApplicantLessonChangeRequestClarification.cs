using Microsoft.AspNetCore.Mvc.Diagnostics;
using WebFormular.Enum;

namespace WebFormular.Classes.Elements.ApplicantLessonChangeRequest
{
    public class ApplicantLessonChangeRequestClarification
    {
        public DateTime? Date { get; set; }
        public Weekdays? Weekday { get; set; }
        public int? TeachingPeriod { get; set; }
        public int? RoomNumber1 { get; set; }
        public LessonChangeRequestType? SelectedType { get; set; }
        public DateTime? SelectedDate  { get; set; }
        public int? SelectedTeachingPeriod { get; set; }

    }
}
