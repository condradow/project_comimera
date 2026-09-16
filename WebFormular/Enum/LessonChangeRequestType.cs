using System.ComponentModel.DataAnnotations;

namespace WebFormular.Enum
{
    public enum LessonChangeRequestType
    {
        [Display(Name = "vorgehalten")]
        BroughtForward,
        [Display(Name = "nachgehalten")]
        Postponed,
        [Display(Name = "Tausch mit")]
        Exchanged
    }
}
