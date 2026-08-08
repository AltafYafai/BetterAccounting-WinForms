using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BetterAccounting.UI.Views
{
    public partial class GstSlabView : Window
    {
        public GstSlabView()
        {
            InitializeComponent();
            DataContext = new ViewModels.GstSlabViewModel();
        }
    }
}
