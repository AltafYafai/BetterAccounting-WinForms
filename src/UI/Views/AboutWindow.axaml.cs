using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BetterAccounting.UI.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            var viewModel = new ViewModels.AboutViewModel();
            viewModel.OnClose = Close;
            DataContext = viewModel;
        }
    }
}
