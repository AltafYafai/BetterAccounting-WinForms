using BetterAccounting.Core.Data.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Data
{
    public class GstinLookupService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        public GstinLookupService(HttpClient httpClient, string endpoint = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _endpoint = endpoint ?? "https://cloud.octagst.com/api/octa/gst/getgstin?gstin=";
        }

        public virtual async Task<GstinLookupResult> LookupAsync(string gstin)
        {
            var result = new GstinLookupResult { Gstin = gstin?.Trim() ?? "" };

            if (string.IsNullOrWhiteSpace(gstin) || gstin.Length != 15)
            {
                result.ErrorMessage = "Invalid GSTIN. Enter a valid 15-character GSTIN.";
                return result;
            }

            try
            {
                var url = _endpoint + Uri.EscapeDataString(gstin.Trim());
                using var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    result.ErrorMessage = $"Lookup failed (HTTP {(int)response.StatusCode}).";
                    return result;
                }

                var payload = JsonSerializer.Deserialize<OctaResponse>(json);
                if (payload == null || !payload.Success || payload.Result == null)
                {
                    result.ErrorMessage = payload?.Errors?.FirstOrDefault()?.Message ?? "GSTIN not found.";
                    return result;
                }

                var found = payload.Result;
                result.Gstin = found.Gstin ?? result.Gstin;
                result.LegalName = found.LegalName ?? "";
                result.TradeName = found.TradeName ?? "";

                if (found.PrincipalAddr != null)
                {
                    result.Address = JoinNonEmpty(", ",
                        found.PrincipalAddr.BuildingName,
                        found.PrincipalAddr.Street,
                        found.PrincipalAddr.Locality);
                    result.City = found.PrincipalAddr.City
                                  ?? found.PrincipalAddr.District ?? "";
                    result.State = found.PrincipalAddr.StateName ?? "";
                    result.PinCode = found.PrincipalAddr.PinCode ?? "";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Lookup failed: {ex.Message}";
            }

            return result;
        }

        private static string JoinNonEmpty(string separator, params string[] values)
        {
            var parts = values.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
            return string.Join(separator, parts);
        }

        private sealed class OctaResponse
        {
            public bool Success { get; set; }
            public OctaResult? Result { get; set; }
            public OctaError[]? Errors { get; set; }
        }

        private sealed class OctaResult
        {
            public string? Gstin { get; set; }
            public string? LegalName { get; set; }
            public string? TradeName { get; set; }
            public OctaAddress? PrincipalAddr { get; set; }
        }

        private sealed class OctaAddress
        {
            public string? BuildingName { get; set; }
            public string? Street { get; set; }
            public string? Locality { get; set; }
            public string? City { get; set; }
            public string? District { get; set; }
            public string? StateName { get; set; }
            public string? PinCode { get; set; }
        }

        private sealed class OctaError
        {
            public string? Message { get; set; }
        }
    }
}