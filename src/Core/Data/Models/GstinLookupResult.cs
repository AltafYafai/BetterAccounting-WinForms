namespace BetterAccounting.Core.Data.Models
{
    public class GstinLookupResult
    {
        public string Gstin { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string TradeName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}