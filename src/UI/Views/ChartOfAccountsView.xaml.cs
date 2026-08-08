using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
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
            try
            {
                var accounts = await _repository.GetAllAsync();
                AccountsGrid.ItemsSource = accounts;
            }
            catch (Exception ex)
            {
                ErrorReporter.Show("Load chart of accounts", ex);
            }
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AccountEditorView(new ViewModels.AccountEditorViewModel(_repository));
            if (dialog.ShowDialog() == true)
                _ = LoadAccountsAsync();
        }

        private void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem is not Core.Services.Data.Account selected)
                return;

            var dialog = new AccountEditorView(new ViewModels.AccountEditorViewModel(_repository, selected));
            if (dialog.ShowDialog() == true)
                _ = LoadAccountsAsync();
        }

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem is not Core.Services.Data.Account selected)
                return;

            var result = MessageBox.Show(
                $"Delete account '{selected.Name}'?\n\nIt will be hidden (soft-deleted) and can be restored by re-adding it.",
                "Delete Account",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _repository.DeleteAsync(selected.Id);
                await LoadAccountsAsync();
            }
            catch (Exception ex)
            {
                ErrorReporter.Show($"Delete account '{selected.Name}'", ex);
            }
        }
    }
}
