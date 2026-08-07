using System;

namespace BetterAccounting.Core.Data.Models
{
    public class CompanyInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "My Company";
        public string DbFilePath { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}