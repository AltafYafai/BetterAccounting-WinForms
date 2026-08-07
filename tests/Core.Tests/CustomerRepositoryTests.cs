using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class CustomerRepositoryTests
    {
        private string _tempDbPath;
        private SQLiteContext _context;
        private CustomerRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _context = new SQLiteContext(_tempDbPath);
            _repository = new CustomerRepository(_context.Connection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            if (File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        [TestMethod]
        public async Task AddThenGetAsync_ShouldPersistCustomer()
        {
            var customer = new Customer
            {
                Name = "ACME PRIVATE LIMITED",
                Gstin = "27AAACS1234A1Z5",
                Address = "1 Main Road",
                City = "Pune",
                State = "Maharashtra",
                PinCode = "411001",
                Phone = "+91-1234567890",
                Email = "billing@acme.example"
            };

            await _repository.AddAsync(customer);

            var customers = await _repository.GetAsync();
            Assert.AreEqual(1, customers.Count);
            var loaded = customers[0];
            Assert.AreEqual("ACME PRIVATE LIMITED", loaded.Name);
            Assert.AreEqual("27AAACS1234A1Z5", loaded.Gstin);
            Assert.AreEqual("Maharashtra", loaded.State);
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldPersistChanges()
        {
            await _repository.AddAsync(new Customer { Name = "Old Name", Gstin = "27AAACS1234A1Z5", Address = "x" });
            var list = await _repository.GetAsync();
            var customer = list.First();

            customer.Name = "New Name";
            customer.City = "Mumbai";
            await _repository.UpdateAsync(customer);

            var loaded = await _repository.GetByIdAsync(customer.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("New Name", loaded!.Name);
            Assert.AreEqual("Mumbai", loaded.City);
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldSoftDeleteCustomer()
        {
            await _repository.AddAsync(new Customer { Name = "ACME", Gstin = "27AAACS1234A1Z5" });
            var customers = await _repository.GetAsync();
            var customer = customers.First();

            await _repository.DeleteAsync(customer.Id);

            var active = await _repository.GetAsync();
            Assert.AreEqual(0, active.Count);

            var all = await _repository.GetAsync(activeOnly: false);
            Assert.AreEqual(1, all.Count);
        }
    }
}