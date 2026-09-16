using WebFormular.Classes.Elements.ApplicantLessonChangeRequest;
using WebFormular.Classes.Elements.LessonArrangementForAbsentClass;
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
            "Unterrichtsabwesenheit",
            typeof(TeachingAbsenceReason),
            typeof(TeachingAbsenceReasonComponent)
        ),

         


         new(
             "Unterrichtstausch",
             typeof(ApplicantLessonChangeRequest),
             typeof(ApplicantLessonChangeRequestComponent)
         ),new(
             "Unterrichtsorganisation bei Abwesenheit der Klasse",
             typeof(LessonArrangementForAbsentClass),
             typeof(LessonArrangementForAbsentClassComponent)
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