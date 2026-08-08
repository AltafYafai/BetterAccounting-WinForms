using System;
using System.Windows;
using BetterAccounting.UI.Models;

namespace BetterAccounting.UI.Views
{
    public partial class ErrorDialog : Window
    {
        private readonly string _report;

        public ErrorDialog(string operation, Exception ex)
        {
            InitializeComponent();
            OperationText.Text = operation;
            MessageText.Text = ErrorReporter.DescribeForDialog(ex);
            _report = ErrorReporter.BuildReport(operation, ex);
            DetailsBox.Text = _report;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_report);
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Copy error report to clipboard", ex);
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = ErrorReporter.BuildMailtoUrl(_report);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Open email app to send error report", ex);
                Copy_Click(sender, e);
                MessageBox.Show(this,
                    "Could not open your email app. The error report was copied to your clipboard instead - paste it into an email to " + ErrorReporter.SupportEmail + ".",
                    "Send Error Report", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
