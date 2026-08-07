using System.Collections.Generic;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Data
{
    public interface IAccountRepository
    {
        Task<List<Account>> GetAllAsync(AccountGroup? group = null);
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByNameAsync(string name);
        Task<bool> ExistsAsync(string name);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(int id);
        Task<List<AccountGroup>> GetAllGroupsAsync();
        Task<List<AccountGroup>> GetGroupHierarchyAsync();
    }
}
