using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using BetterAccounting.UI.Services;

namespace BetterAccounting.UI.Views
{
    public class ErrorDialogWindow : Window
    {
        private readonly string _report;

        public static Task ShowAsync(string operation, string message, string report, Window? owner)
        {
            var dialog = new ErrorDialogWindow(operation, message, report);
            if (owner != null)
                return dialog.ShowDialog(owner);
            dialog.Show();
            return Task.CompletedTask;
        }

        public ErrorDialogWindow(string operation, string message, string report)
        {
            _report = report;

            Title = "BetterAccounting Error";
            Width = 620;
            CanResize = false;
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SizeToContent = SizeToContent.Height;
            MaxHeight = 640;

            var operationText = new TextBlock
            {
                Text = operation,
                FontWeight = FontWeight.Bold,
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.Parse("#C00000")),
                TextWrapping = TextWrapping.Wrap
            };

            var messageText = new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12
            };

            var detailsBox = new TextBox
            {
                Text = report,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                Height = 240,
                Margin = new Thickness(0, 10, 0, 0)
            };
            ScrollViewer.SetHorizontalScrollBarVisibility(detailsBox, ScrollBarVisibility.Auto);
            ScrollViewer.SetVerticalScrollBarVisibility(detailsBox, ScrollBarVisibility.Auto);

            var copyButton = new Button
            {
                Content = "Copy Error",
                MinWidth = 100,
                Padding = new Thickness(12, 4),
                ToolTip.Tip = "Copy the full error details to the clipboard"
            };
            copyButton.Click += async (_, _) =>
            {
                var clipboard = Clipboard;
                if (clipboard != null)
                    await clipboard.SetTextAsync(report);
            };

            var mailButton = new Button
            {
                Content = "Send Error Report",
                MinWidth = 150,
                Padding = new Thickness(12, 4),
                Margin = new Thickness(8, 0, 0, 0),
                ToolTip.Tip = "Opens your email app with the error report ready to send"
            };
            mailButton.Click += (_, _) => SendByEmail();

            var telegramButton = new Button
            {
                Content = "Send to Telegram",
                MinWidth = 150,
                Padding = new Thickness(12, 4),
                Margin = new Thickness(8, 0, 0, 0),
                ToolTip.Tip = "Upload the error report to the Telegram support channel"
            };
            telegramButton.Click += async (_, _) =>
            {
                telegramButton.IsEnabled = false;
                var original = telegramButton.Content;
                telegramButton.Content = "Sending...";
                var ok = await TelegramReporter.SendAsync(report);
                telegramButton.Content = ok ? "Sent ✓" : "Send Failed";
                telegramButton.IsEnabled = true;
            };

            var closeButton = new Button
            {
                Content = "Close",
                MinWidth = 80,
                Padding = new Thickness(12, 4),
                Margin = new Thickness(8, 0, 0, 0),
                IsDefault = true
            };
            closeButton.Click += (_, _) => Close();

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            buttonsPanel.Children.Add(copyButton);
            buttonsPanel.Children.Add(mailButton);
            buttonsPanel.Children.Add(telegramButton);
            buttonsPanel.Children.Add(closeButton);

            var topPanel = new StackPanel();
            topPanel.Children.Add(operationText);
            topPanel.Children.Add(messageText);

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }
                }
            };
            grid.Children.Add(topPanel);
            grid.Children.Add(detailsBox);
            grid.Children.Add(buttonsPanel);
            Grid.SetRow(topPanel, 0);
            Grid.SetRow(detailsBox, 1);
            Grid.SetRow(buttonsPanel, 2);

            Content = new Border
            {
                Padding = new Thickness(16),
                Child = grid
            };
        }

        private void SendByEmail()
        {
            try
            {
                Process.Start(new ProcessStartInfo(Models.ErrorReporter.BuildMailtoUrl(_report))
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                // Email apps are not always available; the copy button still works.
            }
        }
    }
}
