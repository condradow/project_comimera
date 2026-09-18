using Microsoft.AspNetCore.Components;

namespace WebFormular.Classes.Elements.LessonArrangementForAbsentClass
{
    public partial class LessonArrangementForAbsentClassComponent
    {
        [Parameter, EditorRequired]
        public LessonArrangementForAbsentClass Model { get; set; } = null!;

        //TODO könnte man auslagern, wird an mehreren stellen verwendet.
        private DateTime? BeginDate
        {
            get => Model.BeginAbsence;
            set
            {
                if (value.HasValue)
                    Model.BeginAbsence = value.Value;
            }
        }

        private DateTime? EndDate
        {
            get => Model.EndAbsence;
            set
            {
                if (value.HasValue)
                    Model.EndAbsence = value.Value;
            }
        }

        private TimeSpan? EndTime
        {
            get => Model.EndAbsence?.TimeOfDay;

            set
            {
                if (value.HasValue)
                {
                    var date = Model.EndAbsence?.Date ?? DateTime.Today;
                    Model.EndAbsence = date + value.Value;
                }
            }
        }

        private TimeSpan? BeginTime
        {
            get => Model.BeginAbsence?.TimeOfDay;

            set
            {
                if (value.HasValue)
                {
                    var date = Model.BeginAbsence?.Date ?? DateTime.Today;
                    Model.BeginAbsence = date + value.Value;
                }
            }
        }

        protected override Task OnInitializedAsync()
        {
            Model.BeginAbsence ??= DateTime.Today;
            Model.EndAbsence ??= DateTime.Today;
            return base.OnInitializedAsync();
        }

    }
}