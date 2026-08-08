using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BetterAccounting.UI.Views
{
    public partial class BackupRestoreView : Window
    {
        public BackupRestoreView()
        {
            InitializeComponent();
            DataContext = new ViewModels.BackupRestoreViewModel();
        }
    }
}
