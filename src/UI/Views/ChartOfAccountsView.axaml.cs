using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using BetterAccounting.UI.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace BetterAccounting.UI.Views
{
    public partial class ChartOfAccountsView : Window
    {
        private readonly AccountRepository _repository;
        private readonly SqliteConnection _connection;

        public ChartOfAccountsView()
        {
            InitializeComponent();
            var dbPath = BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();
            _connection = new SqliteConnection($"Data Source={dbPath}");
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

        private async void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AccountEditorView(new ViewModels.AccountEditorViewModel(_repository));
            if (await dialog.ShowDialog<bool>(this))
                await LoadAccountsAsync();
        }

        private async void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem is not Account selected)
                return;

            var dialog = new AccountEditorView(new ViewModels.AccountEditorViewModel(_repository, selected));
            if (await dialog.ShowDialog<bool>(this))
                await LoadAccountsAsync();
        }

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem is not Account selected)
                return;

            var result = await MessageBoxService.ShowAsync(
                $"Delete account '{selected.Name}'?\n\nIt will be hidden (soft-deleted) and can be restored by re-adding it.",
                "Delete Account",
                MessageBoxButtons.YesNo,
                MessageBoxImage.Warning,
                AppServices.GetMainWindow());

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
