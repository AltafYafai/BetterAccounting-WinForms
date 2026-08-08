using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using BetterAccounting.UI.Models;
using BetterAccounting.UI.Services;

namespace BetterAccounting.UI.Views
{
    public class DocumentPreviewWindow : Window
    {
        private readonly PrintDocumentModel _document;
        private readonly StackPanel _pagesPanel;
        private readonly TextBlock _zoomLabel;
        private double _zoom = 1.0;

        public DocumentPreviewWindow(PrintDocumentModel document, string title)
        {
            _document = document;
            Title = title;
            Width = 860;
            Height = 640;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.Parse("#E5E5E5"));

            _pagesPanel = new StackPanel { Spacing = 12, Margin = new Thickness(16) };
            foreach (var page in _document.Pages)
            {
                var shadow = new Border
                {
                    Child = page,
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
                    BorderThickness = new Thickness(1),
                    BoxShadow = new BoxShadows(new BoxShadow { OffsetX = 2, OffsetY = 3, Blur = 6, Color = Color.FromArgb(0x55, 0, 0, 0) })
                };
                _pagesPanel.Children.Add(shadow);
            }

            _pagesPanel.RenderTransformOrigin = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Relative);
            _pagesPanel.RenderTransform = new ScaleTransform(_zoom, _zoom);

            _zoomLabel = new TextBlock
            {
                Text = "100%",
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 44,
                TextAlignment = TextAlignment.Center
            };

            var scrollViewer = new ScrollViewer
            {
                Content = _pagesPanel,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var toolbar = BuildToolbar();

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(44) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                }
            };
            grid.Children.Add(toolbar);
            grid.Children.Add(scrollViewer);
            Grid.SetRow(toolbar, 0);
            Grid.SetRow(scrollViewer, 1);

            Content = grid;
        }

        private Control BuildToolbar()
        {
            var titleText = new TextBlock
            {
                Text = Title ?? "Print Preview",
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 16, 0)
            };

            var zoomOut = MakeToolbarButton("\uE8A2", OnZoomOut); // ZoomOut
            var zoomIn = MakeToolbarButton("\uE8A3", OnZoomIn);   // ZoomIn

            var printButton = new Button
            {
                Content = "Print",
                Padding = new Thickness(14, 5),
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            printButton.Click += OnPrint;

            var closeButton = new Button
            {
                Content = "Close",
                Padding = new Thickness(14, 5),
                Margin = new Thickness(8, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            closeButton.Click += (_, _) => Close();

            var panel = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Background = Brushes.White
            };

            panel.Children.Add(titleText); Grid.SetColumn(titleText, 0);
            panel.Children.Add(zoomOut); Grid.SetColumn(zoomOut, 1);
            panel.Children.Add(_zoomLabel); Grid.SetColumn(_zoomLabel, 2);
            panel.Children.Add(zoomIn); Grid.SetColumn(zoomIn, 3);
            panel.Children.Add(printButton); Grid.SetColumn(printButton, 5);
            panel.Children.Add(closeButton); Grid.SetColumn(closeButton, 6);

            return panel;
        }

        private static Button MakeToolbarButton(string glyph, EventHandler<RoutedEventArgs> handler)
        {
            var button = new Button
            {
                Content = new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets") },
                Width = 34,
                Height = 34,
                Padding = new Thickness(0),
                Margin = new Thickness(2, 0, 2, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            button.Click += handler;
            return button;
        }

        private void OnZoomOut(object? sender, RoutedEventArgs e) => SetZoom(_zoom - 0.1);
        private void OnZoomIn(object? sender, RoutedEventArgs e) => SetZoom(_zoom + 0.1);

        private void SetZoom(double zoom)
        {
            _zoom = Math.Clamp(zoom, 0.3, 3.0);
            _pagesPanel.RenderTransform = new ScaleTransform(_zoom, _zoom);
            _zoomLabel.Text = $"{Math.Round(_zoom * 100)}%";
        }

        private async void OnPrint(object? sender, RoutedEventArgs e)
        {
            try
            {
                var path = await PdfExportService.ExportAsync(_document, "document");
                if (path != null)
                    PdfExportService.OpenDocument(path);
            }
            catch (Exception ex)
            {
                ErrorReporter.Show("Print document", ex);
            }
        }
    }
}
