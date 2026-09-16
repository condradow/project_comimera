using WebFormular.Classes.Elements.ApplicantLessonChangeRequest;
using WebFormular.Classes.Elements.LessonArrangementForAbsentClass;
using WebFormular.Classes.Elements.LessonArrangementForAbsentClassAndTeacher;
using WebFormular.Classes.Elements.OfficialDutyDuringStudentAbsence;
using WebFormular.Classes.Elements.OffSiteSchoolEvent;
using WebFormular.Classes.Elements.OtherSchoolEvent;
using WebFormular.Classes.Elements.TeachingAbsenceReason;

public record ElementDefinition(
string Name,
Type ModelType,
Type ComponentType);

public static class ElementRegistry
{
    public static readonly List<ElementDefinition> Elements =
    [
        new(
            "2 /2.1 Unterrichtsabwesenheit",
            typeof(TeachingAbsenceReason),
            typeof(TeachingAbsenceReasonComponent)
        ),




         new(
             "2.2 Unterrichtstausch",
             typeof(ApplicantLessonChangeRequest),
             typeof(ApplicantLessonChangeRequestComponent)
         ),new(
             "3 Unterrichtsorganisation bei Abwesenheit der Klasse",
             typeof(LessonArrangementForAbsentClass),
             typeof(LessonArrangementForAbsentClassComponent)
         ),new(
             "4.1 Schulveranstaltung an außerschulischem Lernort gem. Schulfahrtenerlass",
             typeof(OffSiteSchoolEvent),
             typeof(OffSiteSchoolEventComponent)
         ),new(
             "4 Unterrichtsorganisation bei Abwesenheit von Klasse(n) und Lehrer(n)",
             typeof(LessonArrangementForAbsentClassAndTeacher),
             typeof(LessonArrangementForAbsentClassAndTeacherComponent)
          ),new(
             "4.2 Sonstige Veranstaltungen",
             typeof(OtherSchoolEvent),
             typeof(OtherSchoolEventComponent)
         ),new(
             "5. Anderweitige dienstliche Tätigkeit auf Anordnung des SL bei Abwesenheit der\r\nSuS (vgl. MArbErl SL Pkt 3.3)",
             typeof(OfficialDutyDuringStudentAbsence),
             typeof(OfficialDutyDuringStudentAbsenceComponent)
         )
        //,
        //
        // new(
        //     "Datum",
        //     typeof(DateElement),
        //     typeof(DateElementComponent)
        // )
    ];
}