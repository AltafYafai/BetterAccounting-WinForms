using System.Windows;

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
