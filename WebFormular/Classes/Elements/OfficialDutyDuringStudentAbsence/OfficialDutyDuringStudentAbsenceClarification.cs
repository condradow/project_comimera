using System.Security.Cryptography.Xml;

namespace WebFormular.Classes.Elements.OfficialDutyDuringStudentAbsence
{
    public class OfficialDutyDuringStudentAbsenceClarification
    {
        public DateTime? Date { get; set; }
        public int? TeachingPeriod { get; set; }
        public string OtherOfficialDuty { get; set; } = string.Empty;
        public byte[] SignatureTeacher { get; set; } = [];
        public byte[] SignatureDepartmentHead { get; set; } = [];
        public string Base64SignatureTeacher => System.Text.Encoding.UTF8.GetString(SignatureTeacher);
        public string Base64SignatureDepartmentHead => System.Text.Encoding.UTF8.GetString(SignatureDepartmentHead);


    }
}
