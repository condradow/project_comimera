using System.ComponentModel.DataAnnotations;

namespace WebFormular.Enum;

public enum DocumentStatus
{
    [Display(Name = "Eingereicht")]
    Submitted,
    [Display(Name = "vom AL unterschrieben")]
    SignedByDepartmentHead,
    [Display(Name = "vom stv. SL unterschrieben")]
    SignedByDeputyHeadteacher,
    [Display(Name = "vom SL unterschrieben")]
    SignedByPrincipal
}