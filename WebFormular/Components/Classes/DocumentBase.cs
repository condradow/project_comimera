namespace WebFormular.Components.Classes
{
    public class DocumentBase
    {
        public DateTime CreatedOn { set; get; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Boolean IsInUntis { get; set; }
        public Boolean ApprovedBySubstitue { get;set; }
        public Boolean ApprovedByPrincipal { get; set; }
        public User Teacher { get; set; }
        public string Title { get; set; }
    }
}
