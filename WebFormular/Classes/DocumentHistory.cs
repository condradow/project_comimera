namespace WebFormular.Classes
{
    public class DocumentHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserName { get; set; } = string.Empty;

        public string DocumentName { get; set; } = string.Empty;

        public string Activity { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
