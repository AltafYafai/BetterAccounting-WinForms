using System.Windows;

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
