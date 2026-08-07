using System.Windows;

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

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedItem == null)
                return;

            var result = MessageBox.Show(
                $"Remove '{_viewModel.SelectedItem.Name}'?\n\nIts data will be moved to the 'RemovedCompanies' folder (not deleted) so it can be recovered later.",
                "Remove Company",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                _viewModel.RemoveSelected();
        }
    }
}