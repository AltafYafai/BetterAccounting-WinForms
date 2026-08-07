using System.Windows;
using BetterAccounting.UI.Models;

namespace BetterAccounting.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.Initialize();
        }
    }
}
