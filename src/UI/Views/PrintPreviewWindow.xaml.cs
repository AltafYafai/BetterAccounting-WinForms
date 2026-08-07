using BetterAccounting.Core.Data.Models;
using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class PrintPreviewWindow : Window
    {
        public PrintPreviewWindow(LedgerEntry entry)
        {
            InitializeComponent();
            DataContext = new ViewModels.PrintPreviewViewModel(entry);
        }
    }
}