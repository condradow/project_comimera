namespace WebFormular.Classes.Elements.FinalSigningElement;

public class FinalSigningElement:ElementBase
{
    public byte[] SignatureClassTeacher { get; set; } = [];
    public byte[] SignaturePetitioner { get; set; } = [];
    public byte[] SignatureDepartmentHead { get; set; } = [];
}