using System.ComponentModel.DataAnnotations;

namespace WebFormular.Enum
{
    public enum Weekdays
    {
        [Display(Name = "Montag")]
        Monday,
        [Display(Name = "Dienstag")]
        Thuesday,
        [Display(Name = "Mittwoch")]
        Wednesday,
        [Display(Name = "Donnerstag")]
        Thursday,
        [Display(Name = "Freitag")]
        Friday,
        [Display(Name = "Samstag")]
        Saturday,
        [Display(Name = "Sonntag")]
        Sunday
    }
}
