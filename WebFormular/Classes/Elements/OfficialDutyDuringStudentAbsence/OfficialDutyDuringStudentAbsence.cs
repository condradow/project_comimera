namespace WebFormular.Classes.Elements.OfficialDutyDuringStudentAbsence
{
    public class OfficialDutyDuringStudentAbsence:ElementBase
    {   
        //TODO könnte man als Dynamische Liste erstellen

        public OfficialDutyDuringStudentAbsenceClarification Entry1 { get; set; } = new();
        public OfficialDutyDuringStudentAbsenceClarification Entry2 { get; set; } = new();
        public OfficialDutyDuringStudentAbsenceClarification Entry3 { get; set; } = new();
        public OfficialDutyDuringStudentAbsenceClarification Entry4 { get; set; } = new();
        public OfficialDutyDuringStudentAbsenceClarification Entry5 { get; set; } = new();
    }
}
