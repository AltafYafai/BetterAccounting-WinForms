namespace BetterAccounting.Core.Data.Models
{
    public class CompanyProfile
    {
        public int Id { get; set; } = 1;
        public string CompanyName { get; set; } = string.Empty;
        public string Gstin { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
    }
}