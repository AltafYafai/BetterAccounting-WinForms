using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class AddCustomerView : Window
    {
        public AddCustomerView()
        {
            InitializeComponent();
            DataContext = new ViewModels.AddCustomerViewModel();
        }
    }
}