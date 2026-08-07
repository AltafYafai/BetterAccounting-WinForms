using BetterAccounting.Core.Data.Models;
using Microsoft.Data.Sqlite;

namespace BetterAccounting.Core.Services.Data
{
    public class CustomerRepository
    {
        private readonly SqliteConnection _connection;

        public CustomerRepository(SqliteConnection connection)
        {
            _connection = connection;
            Initialize();
        }

        private void Initialize()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Gstin TEXT NOT NULL,
                    Address TEXT NOT NULL,
                    City TEXT NOT NULL,
                    State TEXT NOT NULL,
                    PinCode TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    IsActive INTEGER DEFAULT 1,
                    CreatedAt TEXT NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
        }

        public async Task<List<Customer>> GetAsync(bool activeOnly = true)
        {
            var customers = new List<Customer>();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = activeOnly
                ? "SELECT * FROM Customers WHERE IsActive = 1 ORDER BY Name"
                : "SELECT * FROM Customers ORDER BY Name";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                customers.Add(MapToCustomer(reader));
            }
            return customers;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Customers WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapToCustomer(reader) : null;
        }

        public async Task AddAsync(Customer customer)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Customers (Name, Gstin, Address, City, State, PinCode, Phone, Email, IsActive, CreatedAt)
                VALUES ($name, $gstin, $address, $city, $state, $pinCode, $phone, $email, $isActive, $createdAt)";
            AddParameters(cmd, customer);
            await cmd.ExecuteNonQueryAsync();

            var idCmd = _connection.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid()";
            customer.Id = (int)(long)(await idCmd.ExecuteScalarAsync() ?? 0L);
        }

        public async Task UpdateAsync(Customer customer)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Customers SET
                    Name = $name,
                    Gstin = $gstin,
                    Address = $address,
                    City = $city,
                    State = $state,
                    PinCode = $pinCode,
                    Phone = $phone,
                    Email = $email,
                    IsActive = $isActive
                WHERE Id = $id";
            AddParameters(cmd, customer);
            cmd.Parameters.AddWithValue("$id", customer.Id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE Customers SET IsActive = 0 WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        private void AddParameters(SqliteCommand cmd, Customer customer)
        {
            cmd.Parameters.AddWithValue("$name", customer.Name);
            cmd.Parameters.AddWithValue("$gstin", customer.Gstin);
            cmd.Parameters.AddWithValue("$address", customer.Address);
            cmd.Parameters.AddWithValue("$city", customer.City);
            cmd.Parameters.AddWithValue("$state", customer.State);
            cmd.Parameters.AddWithValue("$pinCode", customer.PinCode);
            cmd.Parameters.AddWithValue("$phone", customer.Phone);
            cmd.Parameters.AddWithValue("$email", customer.Email);
            cmd.Parameters.AddWithValue("$isActive", customer.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("$createdAt", customer.CreatedAt.ToString("o"));
        }

        private static Customer MapToCustomer(SqliteDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
            Gstin = reader.IsDBNull(reader.GetOrdinal("Gstin")) ? string.Empty : reader.GetString(reader.GetOrdinal("Gstin")),
            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? string.Empty : reader.GetString(reader.GetOrdinal("Address")),
            City = reader.IsDBNull(reader.GetOrdinal("City")) ? string.Empty : reader.GetString(reader.GetOrdinal("City")),
            State = reader.IsDBNull(reader.GetOrdinal("State")) ? string.Empty : reader.GetString(reader.GetOrdinal("State")),
            PinCode = reader.IsDBNull(reader.GetOrdinal("PinCode")) ? string.Empty : reader.GetString(reader.GetOrdinal("PinCode")),
            Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? string.Empty : reader.GetString(reader.GetOrdinal("Phone")),
            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")))
        };
    }
}