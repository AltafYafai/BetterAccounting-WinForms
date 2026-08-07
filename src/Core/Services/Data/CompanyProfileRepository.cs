using BetterAccounting.Core.Data.Models;
using Microsoft.Data.Sqlite;

namespace BetterAccounting.Core.Services.Data
{
    public class CompanyProfileRepository
    {
        private const int RowId = 1;
        private readonly SqliteConnection _connection;

        public CompanyProfileRepository(SqliteConnection connection)
        {
            _connection = connection;
            Initialize();
        }

        private void Initialize()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CompanyProfile (
                    Id INTEGER PRIMARY KEY,
                    CompanyName TEXT NOT NULL,
                    Gstin TEXT NOT NULL,
                    Address TEXT NOT NULL,
                    City TEXT NOT NULL,
                    State TEXT NOT NULL,
                    PinCode TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    ContactPerson TEXT NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
        }

        public async Task<CompanyProfile?> GetAsync()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM CompanyProfile WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", RowId);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapToProfile(reader) : null;
        }

        public async Task SaveAsync(CompanyProfile profile)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO CompanyProfile
                    (Id, CompanyName, Gstin, Address, City, State, PinCode, Phone, Email, ContactPerson)
                VALUES
                    ($id, $companyName, $gstin, $address, $city, $state, $pinCode, $phone, $email, $contactPerson)";
            cmd.Parameters.AddWithValue("$id", RowId);
            cmd.Parameters.AddWithValue("$companyName", profile.CompanyName);
            cmd.Parameters.AddWithValue("$gstin", profile.Gstin);
            cmd.Parameters.AddWithValue("$address", profile.Address);
            cmd.Parameters.AddWithValue("$city", profile.City);
            cmd.Parameters.AddWithValue("$state", profile.State);
            cmd.Parameters.AddWithValue("$pinCode", profile.PinCode);
            cmd.Parameters.AddWithValue("$phone", profile.Phone);
            cmd.Parameters.AddWithValue("$email", profile.Email);
            cmd.Parameters.AddWithValue("$contactPerson", profile.ContactPerson);
            await cmd.ExecuteNonQueryAsync();
        }

        private static CompanyProfile MapToProfile(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CompanyName = GetString(reader, "CompanyName"),
            Gstin = GetString(reader, "Gstin"),
            Address = GetString(reader, "Address"),
            City = GetString(reader, "City"),
            State = GetString(reader, "State"),
            PinCode = GetString(reader, "PinCode"),
            Phone = GetString(reader, "Phone"),
            Email = GetString(reader, "Email"),
            ContactPerson = GetString(reader, "ContactPerson")
        };

        private static string GetString(SqliteDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }
    }
}