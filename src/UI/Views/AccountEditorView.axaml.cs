using Avalonia.Controls;

namespace BetterAccounting.UI.Views
{
    public partial class AccountEditorView : Window
    {
        public AccountEditorView(ViewModels.AccountEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.OnSaved = () => Close(true);
            viewModel.OnClose = Close;
        }
    }
}
