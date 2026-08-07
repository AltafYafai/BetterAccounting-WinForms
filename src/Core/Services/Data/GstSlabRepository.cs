using BetterAccounting.Core.Data.Models;
using Microsoft.Data.Sqlite;

namespace BetterAccounting.Core.Services.Data
{
    public class GstSlabRepository
    {
        private readonly SqliteConnection _connection;

        public GstSlabRepository(SqliteConnection connection)
        {
            _connection = connection;
            Initialize();
        }

        private void Initialize()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS GstSlabs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Rate REAL NOT NULL,
                    IsActive INTEGER DEFAULT 1,
                    CreatedAt TEXT NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
        }

        public async Task SeedDefaultsAsync()
        {
            var countCmd = _connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(1) FROM GstSlabs";
            if ((long)await countCmd.ExecuteScalarAsync() > 0)
                return;

            foreach (var rate in new[] { 0m, 5m, 12m, 18m, 28m })
            {
                await AddAsync(new GstSlab { Name = $"{rate:0.#}%", Rate = rate });
            }
        }

        public async Task<List<GstSlab>> GetAsync(bool activeOnly = true)
        {
            var slabs = new List<GstSlab>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = activeOnly
                ? "SELECT * FROM GstSlabs WHERE IsActive = 1 ORDER BY Rate"
                : "SELECT * FROM GstSlabs ORDER BY Rate";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                slabs.Add(MapToSlab(reader));
            }
            return slabs;
        }

        public async Task<GstSlab?> GetByIdAsync(int id)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM GstSlabs WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapToSlab(reader) : null;
        }

        public async Task AddAsync(GstSlab slab)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO GstSlabs (Name, Rate, IsActive, CreatedAt)
                VALUES ($name, $rate, $isActive, $createdAt)";
            AddParameters(cmd, slab);
            await cmd.ExecuteNonQueryAsync();

            var idCmd = _connection.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid()";
            slab.Id = (int)(long)await idCmd.ExecuteScalarAsync();
        }

        public async Task UpdateAsync(GstSlab slab)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE GstSlabs SET
                    Name = $name,
                    Rate = $rate,
                    IsActive = $isActive
                WHERE Id = $id";
            AddParameters(cmd, slab);
            cmd.Parameters.AddWithValue("$id", slab.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM GstSlabs WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        private void AddParameters(SqliteCommand cmd, GstSlab slab)
        {
            cmd.Parameters.AddWithValue("$name", slab.Name);
            cmd.Parameters.AddWithValue("$rate", slab.Rate);
            cmd.Parameters.AddWithValue("$isActive", slab.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("$createdAt", slab.CreatedAt.ToString("o"));
        }

        private static GstSlab MapToSlab(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
            Rate = reader.GetDecimal(reader.GetOrdinal("Rate")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")))
        };
    }
}