using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class PrintFormatView : Window
    {
        public PrintFormatView()
        {
            InitializeComponent();
            DataContext = new ViewModels.PrintFormatViewModel();
        }
    }
}