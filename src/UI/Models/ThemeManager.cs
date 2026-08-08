using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Globalization;
using System.Xml;

namespace BetterAccounting.UI.Models
{
    public static class ThemeManager
    {
        private const string THEME_KEY = "AppTheme";
        private const string THEME_FILE = "app.theme";

        public static event Action<string> ThemeChanged;

        public static string CurrentTheme { get; private set; } = "Light";

        public static void ApplyTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName)) themeName = "Light";

            // Load theme resource dictionary based on selection
            var dict = new ResourceDictionary();
            dict.Source = new Uri($"/Themes/{themeName}Theme.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
            CurrentTheme = themeName;
            ThemeChanged?.Invoke(themeName);
        }

        public static void Initialize()
        {
            try
            {
                var themePath = GetThemeConfigPath();
                if (File.Exists(themePath))
                {
                    var savedTheme = File.ReadAllText(themePath).Trim();
                    ApplyTheme(savedTheme);
                }
                else
                {
                    ApplyTheme("Light");
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Write("Load saved theme preference", ex);
                ApplyTheme("Light");
            }
        }

        public static void SaveThemePreference(string themeName)
        {
            var themePath = GetThemeConfigPath();
            var dir = Path.GetDirectoryName(themePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(themePath, themeName);
            CurrentTheme = themeName;
        }

        private static string GetThemeConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "BetterAccounting", THEME_FILE);
        }
    }
}
