using Avalonia.Interactivity;
using BetterAccounting.UI.Services;

namespace BetterAccounting.UI.Views
{
    public partial class CompanyManagerView : Window
    {
        private readonly ViewModels.CompanyManagerViewModel _viewModel;

        public CompanyManagerView(ViewModels.CompanyManagerViewModel? viewModel = null)
        {
            InitializeComponent();
            _viewModel = viewModel ?? new ViewModels.CompanyManagerViewModel();
            _viewModel.OnClose = Close;
            DataContext = _viewModel;
        }

        private async void Remove_Click(object? sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedItem == null)
                return;

            var result = await MessageBoxService.ShowAsync(
                $"Remove '{_viewModel.SelectedItem.Name}'?\n\nIts data will be moved to the 'RemovedCompanies' folder (not deleted) so it can be recovered later.",
                "Remove Company",
                MessageBoxButtons.YesNo,
                MessageBoxImage.Warning,
                AppServices.GetMainWindow());

            if (result != MessageBoxResult.Yes)
                return;

            _viewModel.RemoveSelected();
        }
    }
}
