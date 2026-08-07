using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class TrialBalanceServiceTests
    {
        private string _tempDbPath;
        private SQLiteContext _context;
        private TrialBalanceService _service;

        [TestInitialize]
        public void Setup()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _context = new SQLiteContext(_tempDbPath);
            _service = new TrialBalanceService(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            if (File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        [TestMethod]
        public async Task GenerateTrialBalanceAsync_ShouldAggregateLedgerEntries()
        {
            // Debit Cash 500
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "V-001",
                AccountName = "Cash",
                Type = EntryType.Debit,
                Amount = 500m,
                CreatedAt = DateTime.UtcNow
            });

            // Credit Sales 500
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "V-001",
                AccountName = "Sales",
                Type = EntryType.Credit,
                Amount = 500m,
                CreatedAt = DateTime.UtcNow
            });

            var result = await _service.GenerateTrialBalanceAsync();

            // Should have 2 accounts
            Assert.AreEqual(2, result.Count);

            // Cash should have 500 debit
            var cash = result.First(r => r.AccountName == "Cash");
            Assert.AreEqual(500m, cash.TotalDebits);
            Assert.AreEqual(0m, cash.TotalCredits);

            // Sales should have 500 credit
            var sales = result.First(r => r.AccountName == "Sales");
            Assert.AreEqual(0m, sales.TotalDebits);
            Assert.AreEqual(500m, sales.TotalCredits);

            // Trial balance should balance
            var totalDebits = result.Sum(r => r.TotalDebits);
            var totalCredits = result.Sum(r => r.TotalCredits);
            Assert.AreEqual(totalDebits, totalCredits);
        }

        [TestMethod]
        public async Task GenerateTrialBalanceAsync_WithEmptyData_ShouldReturnEmpty()
        {
            var result = await _service.GenerateTrialBalanceAsync();
            Assert.AreEqual(0, result.Count);
        }
    }
}