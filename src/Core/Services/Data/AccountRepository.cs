using Microsoft.Data.Sqlite;
using BetterAccounting.Core.Data.Models;

namespace BetterAccounting.Core.Services.Data
{
    public class AccountRepository : IAccountRepository
    {
        private readonly SqliteConnection _connection;

        public AccountRepository(SqliteConnection connection)
        {
            _connection = connection;
            Initialize();
        }

        private void Initialize()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Accounts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    Group TEXT NOT NULL,
                    Description TEXT,
                    OpeningBalance REAL DEFAULT 0,
                    OpeningBalanceType TEXT DEFAULT 'Debit',
                    IsActive INTEGER DEFAULT 1,
                    CreatedAt TEXT NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
        }

        public async Task<List<Account>> GetAllAsync(AccountGroup? group = null)
        {
            var accounts = new List<Account>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = group.HasValue
                ? "SELECT * FROM Accounts WHERE IsActive = 1 AND [Group] = $group ORDER BY Name"
                : "SELECT * FROM Accounts WHERE IsActive = 1 ORDER BY Name";
            if (group.HasValue) cmd.Parameters.AddWithValue("$group", group.Value.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                accounts.Add(MapToAccount(reader));
            }
            return accounts;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Accounts WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapToAccount(reader) : null;
        }

        public async Task<Account?> GetByNameAsync(string name)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Accounts WHERE Name = $name";
            cmd.Parameters.AddWithValue("$name", name);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapToAccount(reader) : null;
        }

        public async Task<bool> ExistsAsync(string name)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM Accounts WHERE Name = $name";
            cmd.Parameters.AddWithValue("$name", name);
            return (long)(await cmd.ExecuteScalarAsync() ?? 0L) > 0;
        }

        public async Task AddAsync(Account account)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Accounts (Name, Group, Description, OpeningBalance, OpeningBalanceType, IsActive, CreatedAt)
                VALUES ($name, $group, $desc, $openBal, $openType, $isActive, $createdAt)";
            AddParameters(cmd, account);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Accounts SET 
                    Name = $name,
                    Group = $group,
                    Description = $desc,
                    OpeningBalance = $openBal,
                    OpeningBalanceType = $openType,
                    IsActive = $isActive
                WHERE Id = $id";
            AddParameters(cmd, account);
            cmd.Parameters.AddWithValue("$id", account.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE Accounts SET IsActive = 0 WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<AccountGroup>> GetAllGroupsAsync()
        {
            return Enum.GetValues<AccountGroup>().ToList();
        }

        public async Task<List<AccountGroup>> GetGroupHierarchyAsync()
        {
            return await GetAllGroupsAsync();
        }

        private void AddParameters(SqliteCommand cmd, Account account)
        {
            cmd.Parameters.AddWithValue("$name", account.Name);
            cmd.Parameters.AddWithValue("$group", account.Group.ToString());
            cmd.Parameters.AddWithValue("$desc", account.Description ?? "");
            cmd.Parameters.AddWithValue("$openBal", account.OpeningBalance);
            cmd.Parameters.AddWithValue("$openType", account.OpeningBalanceType.ToString());
            cmd.Parameters.AddWithValue("$isActive", account.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("$createdAt", account.CreatedAt.ToString("o"));
        }

        private static Account MapToAccount(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Group = Enum.Parse<AccountGroup>(reader.GetString(reader.GetOrdinal("Group"))),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            OpeningBalance = reader.GetDecimal(reader.GetOrdinal("OpeningBalance")),
            OpeningBalanceType = Enum.Parse<EntryType>(reader.GetString(reader.GetOrdinal("OpeningBalanceType"))),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")))
        };
    }
}
