using System.Windows;

namespace BetterAccounting.UI.Views
{
    public partial class CatchUpReportView : Window
    {
        public CatchUpReportView()
        {
            InitializeComponent();
            DataContext = new ViewModels.CatchUpReportViewModel();
        }
    }
}