
using WebFormular.Classes.Elements;
using WebFormular.Classes.Elements.FinalSigningElement;
using WebFormular.Enum;

namespace WebFormular.Classes
{
    public class DocumentBase
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { set; get; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Boolean IsInUntis { get; set; }
        public Boolean ApprovedBySubstitue { get; set; }
        public Boolean ApprovedByPrincipal { get; set; }
        public User Teacher { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ElementBase> Elements { get; set; } = [];

        public FinalSigningElement? GetSigningElement()
        {
            foreach (ElementBase element in (this.Elements))
            {
                if (element is FinalSigningElement signatures)
                {
                    return signatures;
                }
            }

            return null;
        }

        public DocumentStatus? GetDocumentStatus()
        {
            FinalSigningElement? element = this.GetSigningElement();
            if (element is null)
            {
                return null;
            }
            if (element.SignaturePetitioner.Length == 0)
            {
                return null;
            }
            if(element.SignatureDepartmentHead.Length == 0)
            {
                return DocumentStatus.Submitted;
            }
            
            if(element.SignaturePrincipal.Length == 0)
            {
                return DocumentStatus.Submitted;
            }
            
            return DocumentStatus.SignedByPrincipal;
        }
    }
}
