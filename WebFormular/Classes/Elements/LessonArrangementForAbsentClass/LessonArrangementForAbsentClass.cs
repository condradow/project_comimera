namespace WebFormular.Classes.Elements.LessonArrangementForAbsentClass
{
    public class LessonArrangementForAbsentClass:ElementBase
    {
        public Boolean InfoToPersonel { get; set; }
        public string AffectedClassOrPart { get; set; } = string.Empty;
        public string AffectedReasoning { get; set; } = string.Empty;
        public DateTime? BeginAbsence { get; set; }
        public DateTime? EndAbsence { get; set; }
    }
}
