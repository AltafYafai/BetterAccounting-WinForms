using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class CompanySettingsView : Window
    {
        public CompanySettingsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.CompanySettingsViewModel();
        }
    }
}