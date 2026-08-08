using System.Windows;
using System.Windows.Controls;
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

        public DocumentPreviewWindow(FixedDocument document, string title = "Print Preview")
        {
            InitializeComponent();
            Title = title;
            FixedViewer.Document = document;
            FixedViewer.Visibility = Visibility.Visible;
            Viewer.Visibility = Visibility.Collapsed;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
