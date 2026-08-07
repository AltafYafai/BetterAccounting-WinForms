using BetterAccounting.Core.Data.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Data
{
    public class SQLiteContext : IDataContext, IDisposable
    {
        private readonly SqliteConnection _connection;
        
        public SqliteConnection Connection => _connection;

        public SQLiteContext(string dbPath)
        {
            var parentDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);

            _connection = new SqliteConnection($"Data Source={dbPath}");
            _connection.Open();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS LedgerEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    VoucherNo TEXT NOT NULL,
                    AccountName TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    Description TEXT,
                    ReferenceVoucherId TEXT,
                    CreatedAt TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_date ON LedgerEntries(Date);
                CREATE INDEX IF NOT EXISTS idx_account ON LedgerEntries(AccountName);
                CREATE INDEX IF NOT EXISTS idx_voucher ON LedgerEntries(VoucherNo);
            ";
            cmd.ExecuteNonQuery();
        }

        public async Task<List<LedgerEntry>> GetAllEntriesAsync()
        {
            var entries = new List<LedgerEntry>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM LedgerEntries ORDER BY Date DESC";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(MapRow(reader));
            }
            return entries;
        }

        public async Task AddEntryAsync(LedgerEntry entry)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO LedgerEntries (Date, VoucherNo, AccountName, Type, Amount, Description, ReferenceVoucherId, CreatedAt)
                VALUES ($date, $voucherNo, $accountName, $type, $amount, $description, $refId, $createdAt)";
            cmd.Parameters.AddWithValue("$date", entry.Date.ToString("o"));
            cmd.Parameters.AddWithValue("$voucherNo", entry.VoucherNo);
            cmd.Parameters.AddWithValue("$accountName", entry.AccountName);
            cmd.Parameters.AddWithValue("$type", entry.Type.ToString());
            cmd.Parameters.AddWithValue("$amount", entry.Amount);
            cmd.Parameters.AddWithValue("$description", entry.Description ?? "");
            cmd.Parameters.AddWithValue("$refId", entry.ReferenceVoucherId ?? "");
            cmd.Parameters.AddWithValue("$createdAt", entry.CreatedAt.ToString("o"));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<LedgerEntry>> GetEntriesByDateRangeAsync(DateTime from, DateTime to)
        {
            var entries = new List<LedgerEntry>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM LedgerEntries WHERE Date BETWEEN $from AND $to ORDER BY Date DESC";
            cmd.Parameters.AddWithValue("$from", from.ToString("o"));
            cmd.Parameters.AddWithValue("$to", to.ToString("o"));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(MapRow(reader));
            }
            return entries;
        }

        public async Task<List<LedgerEntry>> GetEntriesByAccountAsync(string accountName)
        {
            var entries = new List<LedgerEntry>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM LedgerEntries WHERE Lower(AccountName) = Lower($accountName) ORDER BY Date DESC";
            cmd.Parameters.AddWithValue("$accountName", accountName);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(MapRow(reader));
            }
            return entries;
        }

        public async Task<List<string>> GetAllAccountNamesAsync()
        {
            var names = new List<string>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT AccountName FROM LedgerEntries ORDER BY AccountName";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                names.Add(reader.GetString(0));
            }
            return names;
        }

        private static LedgerEntry MapRow(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            Date = DateTime.Parse(reader.GetString(reader.GetOrdinal("Date"))),
            VoucherNo = reader.GetString(reader.GetOrdinal("VoucherNo")),
            AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
            Type = Enum.Parse<EntryType>(reader.GetString(reader.GetOrdinal("Type"))),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            ReferenceVoucherId = reader.IsDBNull(reader.GetOrdinal("ReferenceVoucherId")) ? null : reader.GetString(reader.GetOrdinal("ReferenceVoucherId")),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")))
        };

        public void Dispose() => _connection?.Dispose();
    }
}
