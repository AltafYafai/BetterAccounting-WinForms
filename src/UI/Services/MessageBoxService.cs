using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace BetterAccounting.UI.Services
{
    public enum MessageBoxButtons { OK, OKCancel, YesNo }

    public enum MessageBoxResult { None, OK, Cancel, Yes, No }

    public enum MessageBoxImage { None, Information, Warning, Error, Question }

    public static class MessageBoxService
    {
        public static async Task<MessageBoxResult> ShowAsync(
            string message,
            string title = "BetterAccounting",
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxImage image = MessageBoxImage.Information,
            Window? owner = null)
        {
            var window = new Window
            {
                Title = title,
                Width = 440,
                SizeToContent = SizeToContent.Height,
                CanResize = false,
                ShowInTaskbar = false,
                WindowStartupLocation = owner != null
                    ? WindowStartupLocation.CenterOwner
                    : WindowStartupLocation.CenterScreen
            };

            var result = MessageBoxResult.None;

            var icon = new TextBlock
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 26,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 14, 0)
            };

            switch (image)
            {
                case MessageBoxImage.Information:
                    icon.Text = "\uE946";
                    icon.Foreground = Brushes.DodgerBlue;
                    break;
                case MessageBoxImage.Warning:
                    icon.Text = "\uE7BA";
                    icon.Foreground = Brushes.Orange;
                    break;
                case MessageBoxImage.Error:
                    icon.Text = "\uEA39";
                    icon.Foreground = Brushes.IndianRed;
                    break;
                case MessageBoxImage.Question:
                    icon.Text = "\uEA18";
                    icon.Foreground = Brushes.DodgerBlue;
                    break;
                default:
                    icon.IsVisible = false;
                    break;
            }

            var messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13
            };

            var topPanel = new StackPanel { Orientation = Orientation.Horizontal };
            topPanel.Children.Add(icon);
            topPanel.Children.Add(messageText);

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            };

            void AddButton(string text, MessageBoxResult value, bool isDefault = false, bool isCancel = false)
            {
                var button = new Button
                {
                    Content = text,
                    MinWidth = 90,
                    IsDefault = isDefault,
                    IsCancel = isCancel
                };
                button.Click += (_, _) =>
                {
                    result = value;
                    window.Close();
                };
                buttonsPanel.Children.Add(button);
            }

            switch (buttons)
            {
                case MessageBoxButtons.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, isDefault: true);
                    AddButton("Cancel", MessageBoxResult.Cancel, isCancel: true);
                    break;
                case MessageBoxButtons.YesNo:
                    AddButton("Yes", MessageBoxResult.Yes, isDefault: true);
                    AddButton("No", MessageBoxResult.No, isCancel: true);
                    break;
                default:
                    AddButton("OK", MessageBoxResult.OK, isDefault: true, isCancel: true);
                    break;
            }

            window.Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 18,
                Children = { topPanel, buttonsPanel }
            };

            if (owner != null)
                await window.ShowDialog(owner);
            else
                window.Show();

            return result;
        }
    }
}
