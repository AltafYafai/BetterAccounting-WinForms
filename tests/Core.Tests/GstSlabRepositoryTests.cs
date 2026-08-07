using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class GstSlabRepositoryTests
    {
        private string _tempDbPath;
        private SQLiteContext _context;
        private GstSlabRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _context = new SQLiteContext(_tempDbPath);
            _repository = new GstSlabRepository(_context.Connection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            if (File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        [TestMethod]
        public async Task AddThenGetAsync_ShouldReturnAllSlabs()
        {
            await _repository.AddAsync(new GstSlab { Name = "5%", Rate = 5m });
            await _repository.AddAsync(new GstSlab { Name = "18%", Rate = 18m });

            var slabs = await _repository.GetAsync();
            Assert.AreEqual(2, slabs.Count);
            Assert.IsTrue(slabs.Any(s => s.Rate == 5m));
            Assert.IsTrue(slabs.Any(s => s.Rate == 18m));
        }

        [TestMethod]
        public async Task SeedDefaultsAsync_ShouldAddStandardSlabsOnce()
        {
            await _repository.SeedDefaultsAsync();
            await _repository.SeedDefaultsAsync();

            var slabs = await _repository.GetAsync();
            Assert.AreEqual(5, slabs.Count);
            var rates = slabs.Select(s => s.Rate).ToList();
            Assert.IsTrue(rates.Contains(0m));
            Assert.IsTrue(rates.Contains(5m));
            Assert.IsTrue(rates.Contains(12m));
            Assert.IsTrue(rates.Contains(18m));
            Assert.IsTrue(rates.Contains(28m));
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldPersistChanges()
        {
            await _repository.AddAsync(new GstSlab { Name = "Old", Rate = 5m });
            var slabs = await _repository.GetAsync();
            var slab = slabs.First();

            slab.Name = "Old modified";
            slab.Rate = 6m;
            await _repository.UpdateAsync(slab);

            var loaded = await _repository.GetByIdAsync(slab.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("Old modified", loaded!.Name);
            Assert.AreEqual(6m, loaded.Rate);
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldRemoveSlab()
        {
            await _repository.AddAsync(new GstSlab { Name = "28%", Rate = 28m });
            var slabs = await _repository.GetAsync();
            var slab = slabs.First();

            await _repository.DeleteAsync(slab.Id);

            var remaining = await _repository.GetAsync();
            Assert.AreEqual(0, remaining.Count);
        }
    }
}