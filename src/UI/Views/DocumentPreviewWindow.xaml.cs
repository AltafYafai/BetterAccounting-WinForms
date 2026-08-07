using System.Windows;
using System.Windows.Documents;

namespace BetterAccounting.UI.Views
{
    public partial class DocumentPreviewWindow : Window
    {
        public DocumentPreviewWindow(FlowDocument document, string title = "Print Preview")
        {
            InitializeComponent();
            Title = title;
            Viewer.Document = document;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}