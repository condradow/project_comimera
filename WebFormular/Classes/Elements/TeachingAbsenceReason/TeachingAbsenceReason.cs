namespace WebFormular.Classes.Elements.TeachingAbsenceReason
{
    public class TeachingAbsenceReason : ElementBase
    {
        public DateTime BeginAbsence { get; set; }
        public DateTime EndAbsence { get; set; }
        public Boolean MaterialOnTeachingPlatform { get; set; }
        public Boolean MaterialOnLearningPlatform { get; set; }
        public Boolean MaterialOnMail { get; set; }
    }
}
