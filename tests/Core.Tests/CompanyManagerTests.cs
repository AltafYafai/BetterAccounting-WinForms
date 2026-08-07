using BetterAccounting.Core.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class CompanyManagerTests
    {
        private string _appDir;
        private string _legacyPath;

        [TestInitialize]
        public void Setup()
        {
            _appDir = Path.Combine(Path.GetTempPath(), $"ba_companies_{Guid.NewGuid():N}");
            _legacyPath = Path.Combine(_appDir, "data.db");
            Directory.CreateDirectory(_appDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                Directory.Delete(_appDir, true);
            }
            catch
            {
            }
        }

        private CompanyManager CreateManager() => new CompanyManager(_appDir, _legacyPath);

        [TestMethod]
        public void FirstRun_WithoutLegacyDb_CreatesDefaultCompany()
        {
            var manager = CreateManager();

            Assert.AreEqual(1, manager.Companies.Count);
            Assert.IsFalse(string.IsNullOrEmpty(manager.CurrentDbPath));
            Assert.IsTrue(File.Exists(manager.CurrentDbPath));
        }

        [TestMethod]
        public void FirstRun_WithLegacyDb_AdoptsItAsCompany()
        {
            using (var context = new SQLiteContext(_legacyPath))
            {
            }

            var manager = CreateManager();

            Assert.AreEqual(1, manager.Companies.Count);
            Assert.AreEqual(_legacyPath, manager.CurrentDbPath);
        }

        [TestMethod]
        public void CreateCompany_AddsNewEntryAndPersistsRegistry()
        {
            var manager = CreateManager();
            var original = manager.Companies.Count;

            var company = manager.CreateCompany("Client A");
            var reloaded = new CompanyManager(_appDir, _legacyPath);

            Assert.AreEqual(original + 1, manager.Companies.Count);
            Assert.IsTrue(File.Exists(company.DbFilePath));
            Assert.AreEqual(original + 1, reloaded.Companies.Count);
            Assert.IsTrue(reloaded.Companies.Any(c => c.Name == "Client A"));
        }

        [TestMethod]
        public void Switch_ChangesActiveCompanyAndIsPersisted()
        {
            var manager = CreateManager();
            var firstId = manager.ActiveId;
            var second = manager.CreateCompany("Client B");

            manager.Switch(second.Id);
            var reloaded = new CompanyManager(_appDir, _legacyPath);

            Assert.AreNotEqual(firstId, manager.ActiveId);
            Assert.AreEqual(second.Id, reloaded.ActiveId);
        }

        [TestMethod]
        public void Rename_UpdatesCompanyName()
        {
            var manager = CreateManager();
            var company = manager.CreateCompany("Old Name");

            var result = manager.Rename(company.Id, "New Name");

            Assert.IsTrue(result);
            Assert.AreEqual("New Name", manager.Companies.First(c => c.Id == company.Id).Name);
        }

        [TestMethod]
        public void Remove_MovesDatabaseToTrashNotDeletes()
        {
            var manager = CreateManager();
            var company = manager.CreateCompany("Client A");
            var dbPath = company.DbFilePath;

            manager.Remove(company.Id);

            Assert.IsFalse(manager.Companies.Any(c => c.Id == company.Id));
            Assert.IsFalse(File.Exists(dbPath), "The company database should leave the active folder.");
            Assert.IsTrue(Directory.Exists(Path.Combine(_appDir, "RemovedCompanies")));
            Assert.AreEqual(1, Directory.GetFiles(Path.Combine(_appDir, "RemovedCompanies"), "*.db").Length,
                "The database should be preserved in RemovedCompanies (not deleted).");
        }

        [TestMethod]
        public void RemoveLastCompany_RecreatesDefault()
        {
            var manager = CreateManager();

            foreach (var company in manager.Companies.ToList())
                manager.Remove(company.Id);

            Assert.AreEqual(1, manager.Companies.Count);
            Assert.IsFalse(string.IsNullOrEmpty(manager.CurrentDbPath));
            Assert.IsTrue(File.Exists(manager.CurrentDbPath));
        }
    }
}