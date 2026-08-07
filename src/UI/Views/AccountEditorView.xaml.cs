using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class AccountEditorView : Window
    {
        public AccountEditorView(ViewModels.AccountEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.OnSaved = () => DialogResult = true;
            viewModel.OnClose = Close;
        }
    }
}