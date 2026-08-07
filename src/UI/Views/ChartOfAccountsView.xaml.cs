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
        private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

        public ChartOfAccountsView()
        {
            InitializeComponent();
            var dbPath = BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();
            _connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
            _connection.Open();
            _repository = new AccountRepository(_connection);
            Closed += (_, _) => _connection.Dispose();
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
