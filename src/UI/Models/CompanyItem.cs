using System;

namespace BetterAccounting.UI.Models
{
    public class CompanyItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public string DbPath { get; set; } = "";

        public string DisplayName => IsActive ? $"{Name}   (current)" : Name;
    }
}