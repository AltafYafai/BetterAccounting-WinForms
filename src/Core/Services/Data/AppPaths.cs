using System;

namespace BetterAccounting.Core.Services.Data
{
    /// <summary>
    /// Single source of truth for the currently active company's database path.
    /// The BETTER_ACCOUNTING_DB_PATH environment variable overrides company storage
    /// (useful for testing/tooling).
    /// </summary>
    public static class AppPaths
    {
        public static string CurrentDbPath()
        {
            var env = Environment.GetEnvironmentVariable("BETTER_ACCOUNTING_DB_PATH");
            return !string.IsNullOrEmpty(env) ? env : CompanyManager.Instance.CurrentDbPath;
        }
    }
}