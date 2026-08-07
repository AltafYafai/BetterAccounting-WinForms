using BetterAccounting.Core.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class BackupServiceTests
    {
        private string _tempDir;
        private string _tempDbPath;
        private string _backupDir;
        private BackupService _service;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"backup_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDir);
            _tempDbPath = Path.Combine(_tempDir, "test.db");
            _backupDir = Path.Combine(_tempDir, "backups");

            // Create a test database file
            using (var context = new SQLiteContext(_tempDbPath))
            {
                // Close after initialize
            }
            File.WriteAllText(_tempDbPath, "# test database content");

            _service = new BackupService(_tempDbPath, _backupDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [TestMethod]
        public async Task CreateBackupAsync_ShouldCreateZipArchive()
        {
            var backupPath = await _service.CreateBackupAsync("test_backup");
            Assert.IsTrue(File.Exists(backupPath));
            Assert.IsTrue(backupPath.EndsWith(".zip"));
        }

        [TestMethod]
        public async Task CreateBackupAsync_ShouldCreateTimestampedBackup()
        {
            var backupPath = await _service.CreateBackupAsync();
            Assert.IsTrue(File.Exists(backupPath));
            Assert.IsTrue(backupPath.Contains("backup_"));
        }

        [TestMethod]
        public async Task GetAvailableBackupsAsync_ShouldReturnCreatedBackups()
        {
            await _service.CreateBackupAsync("first");
            await _service.CreateBackupAsync("second");

            var backups = _service.GetAvailableBackups();
            Assert.AreEqual(2, backups.Length);
        }

        [TestMethod]
        public async Task RestoreBackupAsync_ShouldRestoreOriginalFile()
        {
            var backupPath = await _service.CreateBackupAsync("restore_test");

            // Modify the original
            File.WriteAllText(_tempDbPath, "modified");

            var success = await _service.RestoreBackupAsync(backupPath);
            Assert.IsTrue(success);

            // Verify content was restored
            var content = File.ReadAllText(_tempDbPath);
            Assert.AreEqual("# test database content", content);
        }

        [TestMethod]
        public async Task RestoreBackupAsync_WithInvalidPath_ShouldReturnFalse()
        {
            var success = await _service.RestoreBackupAsync(Path.Combine(_tempDir, "nonexistent.zip"));
            Assert.IsFalse(success);
        }
    }
}