namespace WebFormular.Components.Classes.Elements
{
    public class ScheduledLessonCancellationReason:ElementBase
    {
        public DateTime BeginAbsence { get; set; }
        public DateTime EndAbsence { get; set; }

        public Boolean MaterialOnTeachingPlatform { get; set; }
        public Boolean MaterialOn
    }
}
