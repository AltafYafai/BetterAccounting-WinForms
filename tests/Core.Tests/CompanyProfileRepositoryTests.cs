using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class CompanyProfileRepositoryTests
    {
        private string _tempDbPath;
        private SQLiteContext _context;
        private CompanyProfileRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _context = new SQLiteContext(_tempDbPath);
            _repository = new CompanyProfileRepository(_context.Connection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            if (File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        [TestMethod]
        public async Task SaveThenGetAsync_ShouldPersistProfile()
        {
            var profile = new CompanyProfile
            {
                CompanyName = "Acme Pvt Ltd",
                Gstin = "27AAACS1234A1Z5",
                Address = "1 Main Road",
                City = "Pune",
                State = "Maharashtra",
                PinCode = "411001",
                Phone = "+91-1234567890",
                Email = "acme@example.com",
                ContactPerson = "Jane Doe"
            };

            await _repository.SaveAsync(profile);

            var loaded = await _repository.GetAsync();
            Assert.IsNotNull(loaded);
            Assert.AreEqual("Acme Pvt Ltd", loaded!.CompanyName);
            Assert.AreEqual("27AAACS1234A1Z5", loaded.Gstin);
            Assert.AreEqual("Pune", loaded.City);
        }

        [TestMethod]
        public async Task SaveTwice_ShouldUpdateSingleRow()
        {
            await _repository.SaveAsync(new CompanyProfile { CompanyName = "First" });
            await _repository.SaveAsync(new CompanyProfile { CompanyName = "Second" });

            var loaded = await _repository.GetAsync();
            Assert.IsNotNull(loaded);
            Assert.AreEqual("Second", loaded!.CompanyName);
        }

        [TestMethod]
        public async Task GetAsync_WithNoProfile_ShouldReturnNull()
        {
            var loaded = await _repository.GetAsync();
            Assert.IsNull(loaded);
        }
    }
}