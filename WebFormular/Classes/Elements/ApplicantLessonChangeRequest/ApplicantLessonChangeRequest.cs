using WebFormular.Enum;

namespace WebFormular.Classes.Elements.ApplicantLessonChangeRequest
{
    public class ApplicantLessonChangeRequest:ElementBase
    {
        public ApplicantLessonChangeRequestClarification Clarification1 {get;set;} = new();
        public ApplicantLessonChangeRequestClarification Clarification2 {get;set;} = new();
        public ApplicantLessonChangeRequestClarification Clarification3 { get; set; } = new();
    }
}
