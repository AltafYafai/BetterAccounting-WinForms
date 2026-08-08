using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BetterAccounting.UI.Views
{
    public partial class SyncSettingsView : Window
    {
        public SyncSettingsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.SyncSettingsViewModel();
        }
    }
}
