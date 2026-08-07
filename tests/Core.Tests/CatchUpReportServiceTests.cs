using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class CatchUpReportServiceTests
    {
        private string _tempDbPath;
        private SQLiteContext _context;
        private AccountRepository _accountRepository;
        private CatchUpReportService _service;

        [TestInitialize]
        public void Setup()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _context = new SQLiteContext(_tempDbPath);
            _accountRepository = new AccountRepository(_context.Connection);
            _service = new CatchUpReportService(_context, _accountRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            if (File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        [TestMethod]
        public async Task GenerateCatchUpAsync_ShouldFlagOverpaidAccounts()
        {
            await _accountRepository.AddAsync(new Account
            {
                Name = "Customer A",
                Group = AccountGroup.Assets,
                OpeningBalance = 0m,
                CreatedAt = DateTime.UtcNow
            });

            // Invoice 400 -> debit
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "INV-001",
                AccountName = "Customer A",
                Type = EntryType.Debit,
                Amount = 400m,
                CreatedAt = DateTime.UtcNow
            });

            // Customer overpays 500 -> credit
            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "RCP-001",
                AccountName = "Customer A",
                Type = EntryType.Credit,
                Amount = 500m,
                CreatedAt = DateTime.UtcNow
            });

            var result = await _service.GenerateCatchUpAsync(DateTime.Today.AddDays(-1), DateTime.Today);

            Assert.AreEqual(1, result.Count);

            var record = result[0];
            Assert.AreEqual("Customer A", record.AccountName);
            Assert.AreEqual(400m, record.Invoiced);
            Assert.AreEqual(500m, record.Paid);
            Assert.AreEqual(-100m, record.ClosingBalance);
            Assert.AreEqual(100m, record.OverPayment);
            Assert.AreEqual("Overpaid", record.Status);
        }

        [TestMethod]
        public async Task GenerateCatchUpAsync_WithNoOverpayment_ShouldBeInBalance()
        {
            await _accountRepository.AddAsync(new Account
            {
                Name = "Customer B",
                Group = AccountGroup.Assets,
                OpeningBalance = 0m,
                CreatedAt = DateTime.UtcNow
            });

            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "INV-002",
                AccountName = "Customer B",
                Type = EntryType.Debit,
                Amount = 200m,
                CreatedAt = DateTime.UtcNow
            });

            await _context.AddEntryAsync(new LedgerEntry
            {
                Date = DateTime.Today,
                VoucherNo = "RCP-002",
                AccountName = "Customer B",
                Type = EntryType.Credit,
                Amount = 200m,
                CreatedAt = DateTime.UtcNow
            });

            var result = await _service.GenerateCatchUpAsync(DateTime.Today.AddDays(-1), DateTime.Today);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0m, result[0].OverPayment);
            Assert.AreEqual("In balance", result[0].Status);
        }
    }
}