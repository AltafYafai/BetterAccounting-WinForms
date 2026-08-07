using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BetterAccounting.UI.Views
{
    public partial class ChartOfAccountsView : Window
    {
        private readonly AccountRepository _repository;

        public ChartOfAccountsView()
        {
            InitializeComponent();
            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BetterAccounting", "data.db");
            _repository = new AccountRepository(new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}"));
            _ = LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync()
        {
            var accounts = await _repository.GetAllAsync();
            AccountsGrid.ItemsSource = accounts;
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            // Would open AccountEditorViewModel dialog
        }

        private void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            // Would edit selected account
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            // Would delete selected account
        }
    }
}
