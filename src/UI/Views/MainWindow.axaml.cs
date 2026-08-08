using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BetterAccounting.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.DashboardViewModel();
        }

        private void OnExitClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
