using Avalonia.Controls;
using Avalonia.Interactivity;

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
