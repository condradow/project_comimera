using Microsoft.AspNetCore.Components;

namespace WebFormular.Classes.Elements.LessonArrangementForAbsentClass
{
    public partial class LessonArrangementForAbsentClassComponent
    {
        [Parameter, EditorRequired]
        public LessonArrangementForAbsentClass Model { get; set; } = null!;

    }
}