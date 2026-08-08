using BetterAccounting.Core.Data.Models;
using BetterAccounting.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BetterAccounting.UI.Views
{
    public partial class PrintPreviewWindow : Window
    {
        public PrintPreviewWindow(LedgerEntry entry)
        {
            InitializeComponent();
            var vm = new PrintPreviewViewModel(entry);
            DataContext = vm;

            if (vm.UsesFixedDocument)
            {
                FixedViewer.Document = vm.FixedDocument;
                FixedViewer.Visibility = Visibility.Visible;
                FlowReader.Visibility = Visibility.Collapsed;
            }
        }
    }
}
