using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.DashboardViewModel();
        }
    }
}
