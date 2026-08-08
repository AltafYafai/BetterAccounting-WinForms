using Avalonia.Controls;
using BetterAccounting.Core.Data.Models;

namespace BetterAccounting.UI.Views
{
    public partial class VoucherEntryView : Window
    {
        public VoucherEntryView() : this(VoucherType.Journal)
        {
        }

        public VoucherEntryView(VoucherType voucherType)
        {
            InitializeComponent();
            DataContext = new ViewModels.VoucherEntryViewModel(voucherType);
        }
    }
}
