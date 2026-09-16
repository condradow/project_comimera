namespace WebFormular.Classes.Elements.OffSiteSchoolEvent
{
    public  class OffSiteSchoolEvent : ElementBase
    {
        public Boolean EducationalExcursion { get; set; }

        public Boolean SchoolTrip { get; set; }

        public Boolean InternationalEvent { get; set; }

        public Boolean SpecialOccasionTrip { get; set; }

        public string Destination { get; set; } = string.Empty;

        //TODO könnte man mit reationaler auswahl machen
        public string AdditionalTeachers { get; set; } = string.Empty;
        public string Transportation { get; set; } = string.Empty;
        public DateTime? BeginAbsence { get; set; }
        public DateTime? EndAbsence { get; set; }
    }
}
