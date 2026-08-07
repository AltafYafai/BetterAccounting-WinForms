using BetterAccounting.Core.Data.Models;
using Microsoft.Data.Sqlite;

namespace BetterAccounting.Core.Services.Data
{
    public class PrintTemplateRepository
    {
        private readonly SqliteConnection _connection;

        public PrintTemplateRepository(SqliteConnection connection)
        {
            _connection = connection;
            Initialize();
        }

        private void Initialize()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS PrintTemplates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    DocumentType TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    IsDefault INTEGER DEFAULT 0,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
        }

        public async Task<List<PrintTemplate>> GetAllAsync(DocumentType? type = null)
        {
            var templates = new List<PrintTemplate>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = type.HasValue
                ? "SELECT * FROM PrintTemplates WHERE DocumentType = $type ORDER BY Name"
                : "SELECT * FROM PrintTemplates ORDER BY DocumentType, Name";
            if (type.HasValue)
                cmd.Parameters.AddWithValue("$type", type.Value.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                templates.Add(MapToTemplate(reader));
            }
            return templates;
        }

        public async Task<PrintTemplate?> GetByIdAsync(int id)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM PrintTemplates WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapToTemplate(reader) : null;
        }

        public async Task<PrintTemplate?> GetDefaultAsync(DocumentType type)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM PrintTemplates WHERE DocumentType = $type AND IsDefault = 1 LIMIT 1";
            cmd.Parameters.AddWithValue("$type", type.ToString());
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapToTemplate(reader) : null;
        }

        public async Task AddAsync(PrintTemplate template)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO PrintTemplates (Name, DocumentType, Content, IsDefault, CreatedAt, UpdatedAt)
                VALUES ($name, $type, $content, $isDefault, $createdAt, $updatedAt)";
            AddParameters(cmd, template);
            await cmd.ExecuteNonQueryAsync();

            var idCmd = _connection.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid()";
            template.Id = (int)(long)(await idCmd.ExecuteScalarAsync() ?? 0L);
        }

        public async Task UpdateAsync(PrintTemplate template)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE PrintTemplates SET
                    Name = $name,
                    DocumentType = $type,
                    Content = $content,
                    IsDefault = $isDefault,
                    UpdatedAt = $updatedAt
                WHERE Id = $id";
            AddParameters(cmd, template);
            cmd.Parameters.AddWithValue("$id", template.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM PrintTemplates WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SetDefaultAsync(int id, DocumentType type)
        {
            var reset = _connection.CreateCommand();
            reset.CommandText = "UPDATE PrintTemplates SET IsDefault = 0 WHERE DocumentType = $type";
            reset.Parameters.AddWithValue("$type", type.ToString());
            await reset.ExecuteNonQueryAsync();

            var set = _connection.CreateCommand();
            set.CommandText = "UPDATE PrintTemplates SET IsDefault = 1 WHERE Id = $id";
            set.Parameters.AddWithValue("$id", id);
            await set.ExecuteNonQueryAsync();
        }

        private void AddParameters(SqliteCommand cmd, PrintTemplate template)
        {
            cmd.Parameters.AddWithValue("$name", template.Name);
            cmd.Parameters.AddWithValue("$type", template.DocumentType.ToString());
            cmd.Parameters.AddWithValue("$content", template.Content);
            cmd.Parameters.AddWithValue("$isDefault", template.IsDefault ? 1 : 0);
            cmd.Parameters.AddWithValue("$createdAt", template.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$updatedAt", template.UpdatedAt.ToString("o"));
        }

        private static PrintTemplate MapToTemplate(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            DocumentType = Enum.Parse<DocumentType>(reader.GetString(reader.GetOrdinal("DocumentType"))),
            Content = reader.IsDBNull(reader.GetOrdinal("Content")) ? "" : reader.GetString(reader.GetOrdinal("Content")),
            IsDefault = reader.GetBoolean(reader.GetOrdinal("IsDefault")),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
        };
    }
}