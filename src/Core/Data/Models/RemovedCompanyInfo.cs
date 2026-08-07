using System;

namespace BetterAccounting.Core.Data.Models
{
    public class RemovedCompanyInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string DbFilePath { get; set; } = "";
        public DateTime? RemovedAt { get; set; }
    }
}