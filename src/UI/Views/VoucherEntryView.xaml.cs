using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class VoucherEntryView : Window
    {
        public VoucherEntryView()
        {
            InitializeComponent();
            DataContext = new ViewModels.VoucherEntryViewModel();
        }
    }
}
