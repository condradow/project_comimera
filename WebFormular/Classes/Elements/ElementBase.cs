
namespace WebFormular.Classes.Elements
{
    public abstract class ElementBase
    {
        public string Title { get; set; }
        public DocumentBase Document { get; set; } = null!;
        public Guid DocumentId { get; set; }
    }
}
