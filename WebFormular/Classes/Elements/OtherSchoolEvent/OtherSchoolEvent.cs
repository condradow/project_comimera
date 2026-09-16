using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebFormular.Classes.Elements.OtherSchoolEvent
{
    public class OtherSchoolEvent:ElementBase
    {
        public Boolean Project { get; set; }

        public Boolean InternationalEvent { get; set; }

        public Boolean Other { get; set; }


        public string TopicOrLocation { get; set; } = string.Empty;

        //TODO könnte man mit reationaler auswahl machen
        public string AdditionalTeachers { get; set; } = string.Empty;
        public string Transportation { get; set; } = string.Empty;
        public DateTime? BeginAbsence { get; set; }
        public DateTime? EndAbsence { get; set; }
    }
}
