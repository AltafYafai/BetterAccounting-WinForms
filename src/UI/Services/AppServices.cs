using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace BetterAccounting.UI.Services
{
    public static class AppServices
    {
        public static Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
                { MainWindow: { } window })
                return window;
            return null;
        }

        public static TopLevel? GetTopLevel() => GetMainWindow();

        public static IStorageProvider? StorageProvider => GetTopLevel()?.StorageProvider;
    }
}
