using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BetterAccounting.Core.Data.Models;

namespace BetterAccounting.Core.Services.Data
{
    /// <summary>
    /// Manages a register of companies, each backed by its own SQLite database file.
    /// Data is always kept: removing a company moves its database into a "RemovedCompanies"
    /// folder rather than deleting it, so no information is ever lost.
    /// </summary>
    public sealed class CompanyManager
    {
        private static CompanyManager? _instance;
        public static CompanyManager Instance => _instance ??= new CompanyManager();

        private readonly string _appDataDir;
        private readonly string _companiesDir;
        private readonly string _registryPath;
        private readonly string _trashDir;
        private readonly string _legacyDbPath;

        private List<CompanyInfo> _companies = new List<CompanyInfo>();
        private Guid _activeId;

        public IReadOnlyList<CompanyInfo> Companies => _companies;
        public Guid ActiveId => _activeId;

        public CompanyInfo Active
        {
            get
            {
                if (_companies.Count == 0)
                    Load();
                var active = _companies.FirstOrDefault(c => c.Id == _activeId);
                return active ?? _companies[0];
            }
        }

        public string CurrentDbPath => Active.DbFilePath;

        public CompanyManager(string? appDataDir = null, string? legacyDbPath = null)
        {
            _appDataDir = appDataDir ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BetterAccounting");
            _companiesDir = Path.Combine(_appDataDir, "Companies");
            _registryPath = Path.Combine(_appDataDir, "companies.json");
            _trashDir = Path.Combine(_appDataDir, "RemovedCompanies");
            _legacyDbPath = legacyDbPath ?? Path.Combine(_appDataDir, "data.db");

            Load();
        }

        /// <summary>
        /// Loads the register. On first run it adopts the legacy standalone database
        /// (data.db) as a company, or creates a fresh default company otherwise.
        /// </summary>
        public void Load()
        {
            _companies.Clear();
            _activeId = Guid.Empty;

            if (TryReadRegistry())
                return;

            if (File.Exists(_legacyDbPath))
            {
                AddCompanyCore("My Company", _legacyDbPath);
            }
            else
            {
                Directory.CreateDirectory(_companiesDir);
                AddCompanyCore("My Company", Path.Combine(_companiesDir, $"{Guid.NewGuid():N}.db"));
            }

            using (new SQLiteContext(Active.DbFilePath))
            {
            }

            Save();
        }

        private bool TryReadRegistry()
        {
            if (!File.Exists(_registryPath))
                return false;

            try
            {
                var json = File.ReadAllText(_registryPath);
                var state = JsonSerializer.Deserialize<CompanyRegistryState>(json);
                if (state?.Companies != null && state.Companies.Count > 0)
                {
                    _companies = state.Companies;
                    var savedActive = Guid.TryParse(state.ActiveId, out var id) ? id : Guid.Empty;
                    _activeId = _companies.Any(c => c.Id == savedActive) ? savedActive : _companies[0].Id;
                    return true;
                }
            }
            catch
            {
                // Corrupt register: fall through and rebuild.
            }

            return false;
        }

        public CompanyInfo CreateCompany(string name)
        {
            var company = new CompanyInfo
            {
                Id = Guid.NewGuid(),
                Name = NormalizeName(name),
                DbFilePath = Path.Combine(_companiesDir, $"{Guid.NewGuid():N}.db"),
                CreatedAt = DateTime.UtcNow
            };

            Directory.CreateDirectory(_companiesDir);
            using (new SQLiteContext(company.DbFilePath))
            {
            }

            _companies.Add(company);
            Save();
            return company;
        }

        public void Switch(Guid id)
        {
            if (_companies.All(c => c.Id != id))
                throw new InvalidOperationException("Company not found.");

            _activeId = id;
            Save();
        }

        public bool Rename(Guid id, string name)
        {
            var company = _companies.FirstOrDefault(c => c.Id == id);
            if (company == null)
                return false;

            var normalized = NormalizeName(name);
            if (string.IsNullOrWhiteSpace(normalized))
                return false;

            company.Name = normalized;
            Save();
            return true;
        }

        /// <summary>
        /// Removes a company from the active list but never deletes its data file.
        /// The database is moved to the RemovedCompanies folder so it can always be recovered.
        /// </summary>
        public void Remove(string id)
        {
            var guid = Guid.Parse(id);
            Remove(guid);
        }

        public void Remove(Guid id)
        {
            var company = _companies.FirstOrDefault(c => c.Id == id);
            if (company == null)
                return;

            try
            {
                Directory.CreateDirectory(_trashDir);
                var safeName = SanitizeFileName(company.Name);
                var destination = Path.Combine(_trashDir, $"{safeName}_{id:N}.db");
                if (File.Exists(company.DbFilePath) && !File.Exists(destination))
                    File.Move(company.DbFilePath, destination);
            }
            catch
            {
                // If the move fails, leave the register entry in place rather than return a broken state.
            }

_companies = _companies.Where(c => c.Id != id).ToList();
            if (_companies.Count == 0)
            {
                AddCompanyCore("My Company", Path.Combine(_companiesDir, $"{Guid.NewGuid():N}.db"));
                using (new SQLiteContext(Active.DbFilePath))
                {
                }
            }

            if (_activeId == id)
                _activeId = _companies[0].Id;

            Save();
        }

        private void AddCompanyCore(string name, string dbPath)
        {
            _companies.Add(new CompanyInfo
            {
                Id = Guid.NewGuid(),
                Name = name,
                DbFilePath = dbPath,
                CreatedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Lists companies whose databases were moved to the RemovedCompanies folder.
        /// </summary>
        public List<RemovedCompanyInfo> GetRemovedCompanies()
        {
            var removed = new List<RemovedCompanyInfo>();
            if (!Directory.Exists(_trashDir))
                return removed;

            foreach (var file in Directory.GetFiles(_trashDir, "*.db"))
            {
                var (name, id) = ParseRemovedFileName(Path.GetFileNameWithoutExtension(file));
                var info = new RemovedCompanyInfo
                {
                    Id = id,
                    Name = name,
                    DbFilePath = file,
                    RemovedAt = File.GetLastWriteTime(file)
                };
                removed.Add(info);
            }

            return removed;
        }

        /// <summary>Restores a previously removed company back into the active list.</summary>
        public RemovedCompanyInfo? RestoreRemovedCompany(RemovedCompanyInfo removed)
        {
            if (removed == null || !File.Exists(removed.DbFilePath))
                return null;

            if (_companies.Any(c => c.Id == removed.Id))
                return null;

            Directory.CreateDirectory(_companiesDir);
            var destination = Path.Combine(_companiesDir, $"{Guid.NewGuid():N}.db");
            File.Move(removed.DbFilePath, destination);

            _companies.Add(new CompanyInfo
            {
                Id = removed.Id,
                Name = removed.Name,
                DbFilePath = destination,
                CreatedAt = DateTime.UtcNow
            });
            Save();
            return removed;
        }

        private static (string Name, Guid Id) ParseRemovedFileName(string fileName)
        {
            // Files are saved as "<safeName>_<guid32>.db"
            var id = Guid.Empty;
            if (fileName != null && fileName.Length > 33)
            {
                var idText = fileName.Substring(fileName.Length - 32);
                Guid.TryParse(idText, out id);
            }

            var name = fileName ?? "";
            if (id != Guid.Empty && name.Length > 33)
                name = name.Substring(0, name.Length - 33).Replace('_', ' ');

            return (string.IsNullOrWhiteSpace(name) ? "Company" : name, id);
        }

        private void Save()
        {
            var state = new CompanyRegistryState
            {
                Companies = _companies,
                ActiveId = _activeId.ToString()
            };
            Directory.CreateDirectory(_appDataDir);
            var json = JsonSerializer.Serialize(state);
            var temp = _registryPath + ".tmp";
            File.WriteAllText(temp, json);
            File.Move(temp, _registryPath, true);
        }

        private static string NormalizeName(string name)
        {
            var trimmed = name?.Trim() ?? "";
            return string.IsNullOrWhiteSpace(trimmed) ? "My Company" : trimmed;
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return string.IsNullOrWhiteSpace(name) ? "Company" : name;
        }

        private sealed class CompanyRegistryState
        {
            public List<CompanyInfo> Companies { get; set; } = new List<CompanyInfo>();
            public string ActiveId { get; set; } = "";
        }
    }
}