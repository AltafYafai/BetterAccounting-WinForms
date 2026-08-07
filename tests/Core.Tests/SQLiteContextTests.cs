using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class SQLiteContextTests
    {
        private string _tempDbPath;
        private SQLiteContext _context;

        [TestInitialize]
        public void Setup()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _context = new SQLiteContext(_tempDbPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            if (File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        [TestMethod]
        public async Task AddEntryAsync_ShouldPersistEntry()
        {
            var entry = new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "V-001",
                AccountName = "Cash",
                Type = EntryType.Debit,
                Amount = 100.50m,
                Description = "Test entry",
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddEntryAsync(entry);

            var entries = await _context.GetAllEntriesAsync();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("V-001", entries[0].VoucherNo);
            Assert.AreEqual(100.50m, entries[0].Amount);
            Assert.AreEqual(EntryType.Debit, entries[0].Type);
        }

        [TestMethod]
        public async Task GetEntriesByAccountAsync_ShouldFilterByAccount()
        {
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "V-001",
                AccountName = "Cash",
                Type = EntryType.Debit,
                Amount = 100m,
                CreatedAt = DateTime.UtcNow
            });
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "V-002",
                AccountName = "Bank",
                Type = EntryType.Credit,
                Amount = 50m,
                CreatedAt = DateTime.UtcNow
            });

            var cashEntries = await _context.GetEntriesByAccountAsync("Cash");
            Assert.AreEqual(1, cashEntries.Count);
            Assert.AreEqual("Cash", cashEntries[0].AccountName);
        }

        [TestMethod]
        public async Task GetEntriesByDateRangeAsync_ShouldReturnDateFiltered()
        {
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today.AddDays(-10),
                VoucherNo = "V-001",
                AccountName = "Cash",
                Type = EntryType.Debit,
                Amount = 100m,
                CreatedAt = DateTime.UtcNow
            });
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "V-002",
                AccountName = "Cash",
                Type = EntryType.Credit,
                Amount = 50m,
                CreatedAt = DateTime.UtcNow
            });

            var from = DateTime.Today.AddDays(-5);
            var to = DateTime.Today.AddDays(1);
            var filtered = await _context.GetEntriesByDateRangeAsync(from, to);

            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("V-002", filtered[0].VoucherNo);
        }

        [TestMethod]
        public async Task GetAllAccountNamesAsync_ShouldReturnDistinctNames()
        {
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "V-001",
                AccountName = "Cash",
                Type = EntryType.Debit,
                Amount = 100m,
                CreatedAt = DateTime.UtcNow
            });
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "V-002",
                AccountName = "Cash",
                Type = EntryType.Credit,
                Amount = 50m,
                CreatedAt = DateTime.UtcNow
            });

            var names = await _context.GetAllAccountNamesAsync();
            Assert.AreEqual(1, names.Count);
            Assert.AreEqual("Cash", names[0]);
        }
    }
}
