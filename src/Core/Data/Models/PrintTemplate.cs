namespace BetterAccounting.Core.Data.Models
{
    public enum DocumentType
    {
        Invoice,
        Ledger,
        Cover,
        Report
    }

    public class PrintTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DocumentType DocumentType { get; set; } = DocumentType.Invoice;
        public string Content { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}