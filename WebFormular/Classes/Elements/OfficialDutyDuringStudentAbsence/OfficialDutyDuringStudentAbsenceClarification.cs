namespace WebFormular.Classes.Elements.OfficialDutyDuringStudentAbsence
{
    public class OfficialDutyDuringStudentAbsenceClarification
    {
        public DateTime? Date { get; set; }
        public int? TeachingPeriod { get; set; }
        public string OtherOfficialDuty { get; set; } = string.Empty;
        public string Base64SignatureTeacher { get; set; }= string.Empty;
        public string Base64SignatureDepartmentHead { get; set; } = string.Empty;
    }
}
