using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class PrintTemplateRepositoryTests
    {
        private string _tempDbPath;
        private SQLiteContext _context;
        private PrintTemplateRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _context = new SQLiteContext(_tempDbPath);
            _repository = new PrintTemplateRepository(_context.Connection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
            if (File.Exists(_tempDbPath))
                File.Delete(_tempDbPath);
        }

        [TestMethod]
        public async Task AddThenGet_ShouldReturnTemplateByType()
        {
            var template = new PrintTemplate
            {
                Name = "Custom Invoice",
                DocumentType = DocumentType.Invoice,
                Content = "Company: {CompanyName}"
            };
            await _repository.AddAsync(template);

            var all = await _repository.GetAllAsync(DocumentType.Invoice);
            Assert.AreEqual(1, all.Count);
            Assert.AreEqual("Custom Invoice", all[0].Name);
            Assert.AreEqual("Company: {CompanyName}", all[0].Content);
        }

        [TestMethod]
        public async Task GetDefault_ShouldReturnDefaultFlaggedTemplate()
        {
            await _repository.AddAsync(new PrintTemplate { Name = "A", DocumentType = DocumentType.Invoice, IsDefault = false });
            await _repository.AddAsync(new PrintTemplate { Name = "B", DocumentType = DocumentType.Invoice, IsDefault = true });

            var def = await _repository.GetDefaultAsync(DocumentType.Invoice);
            Assert.IsNotNull(def);
            Assert.AreEqual("B", def.Name);
        }

        [TestMethod]
        public async Task SetDefault_ShouldUnsetPreviousDefault()
        {
            var a = new PrintTemplate { Name = "A", DocumentType = DocumentType.Ledger, IsDefault = true };
            var b = new PrintTemplate { Name = "B", DocumentType = DocumentType.Ledger };
            await _repository.AddAsync(a);
            await _repository.AddAsync(b);

            await _repository.SetDefaultAsync(b.Id, DocumentType.Ledger);

            var all = await _repository.GetAllAsync(DocumentType.Ledger);
            Assert.AreEqual(false, all.Single(t => t.Id == a.Id).IsDefault);
            Assert.AreEqual(true, all.Single(t => t.Id == b.Id).IsDefault);
        }

        [TestMethod]
        public async Task Delete_ShouldRemoveTemplate()
        {
            var t = new PrintTemplate { Name = "Temp", DocumentType = DocumentType.Cover };
            await _repository.AddAsync(t);

            await _repository.DeleteAsync(t.Id);

            var all = await _repository.GetAllAsync(DocumentType.Cover);
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public async Task Update_ShouldPersistChanges()
        {
            var t = new PrintTemplate { Name = "Old", DocumentType = DocumentType.Report, Content = "a" };
            await _repository.AddAsync(t);

            t.Name = "New";
            t.Content = "b";
            await _repository.UpdateAsync(t);

            var loaded = await _repository.GetByIdAsync(t.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("New", loaded.Name);
            Assert.AreEqual("b", loaded.Content);
        }

        [TestMethod]
        public async Task GetAll_WithoutType_ShouldReturnAllTypes()
        {
            await _repository.AddAsync(new PrintTemplate { Name = "1", DocumentType = DocumentType.Invoice });
            await _repository.AddAsync(new PrintTemplate { Name = "2", DocumentType = DocumentType.Report });

            var all = await _repository.GetAllAsync();
            Assert.AreEqual(2, all.Count);
        }
    }
}