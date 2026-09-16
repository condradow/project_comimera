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
        )

        // Später einfach:
        //
        // new(
        //     "Freitext",
        //     typeof(TextElement),
        //     typeof(TextElementComponent)
        // ),
        //
        // new(
        //     "Datum",
        //     typeof(DateElement),
        //     typeof(DateElementComponent)
        // )
    ];
}