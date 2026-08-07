using BetterAccounting.Core.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Data
{
    public interface IDataContext
    {
        Task<List<LedgerEntry>> GetAllEntriesAsync();
        Task AddEntryAsync(LedgerEntry entry);
        Task<List<LedgerEntry>> GetEntriesByDateRangeAsync(DateTime from, DateTime to);
        Task<List<LedgerEntry>> GetEntriesByAccountAsync(string accountName);
        Task<List<string>> GetAllAccountNamesAsync();
    }
}
