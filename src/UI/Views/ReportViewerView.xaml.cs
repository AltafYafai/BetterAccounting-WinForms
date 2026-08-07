using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class ReportViewerView : Window
    {
        public ReportViewerView()
        {
            InitializeComponent();
            DataContext = new ViewModels.ReportViewerViewModel();
        }
    }
}
