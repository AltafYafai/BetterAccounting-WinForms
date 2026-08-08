using BetterAccounting.Core.Data.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BetterAccounting.UI.Models
{
    public static class ColorBrushConverter
    {
        public static Brush? ToBrush(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (value.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                return Brushes.Transparent;
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
            }
            catch (Exception ex)
            {
                ErrorLog.Write($"Parse color '{value}'", ex);
                return null;
            }
        }
    }

    public static class TemplateItemShapeFactory
    {
        public static (double X, double Y, double Width, double Height) Normalize(PrintTemplateItem item)
        {
            if (item.Kind == TemplateItemKind.Line)
            {
                var x = Math.Min(item.X, item.X + item.Width);
                var y = Math.Min(item.Y, item.Y + item.Height);
                return (x, y, Math.Abs(item.Width), Math.Abs(item.Height));
            }
            return (item.X, item.Y, item.Width, item.Height);
        }

        public static FrameworkElement Create(PrintTemplateItem item, Func<string, string>? substitute = null)
        {
            FrameworkElement element;
            switch (item.Kind)
            {
                case TemplateItemKind.Text:
                {
                    var textBlock = new TextBlock
                    {
                        Text = substitute?.Invoke(item.Text) ?? item.Text,
                        FontFamily = new FontFamily(string.IsNullOrWhiteSpace(item.FontFamily) ? "Segoe UI" : item.FontFamily),
                        FontSize = item.FontSize <= 0 ? 12 : item.FontSize,
                        FontWeight = item.Bold ? FontWeights.Bold : FontWeights.Normal,
                        FontStyle = item.Italic ? FontStyles.Italic : FontStyles.Normal,
                        TextAlignment = MapTextAlignment(item.TextAlignment),
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = MapVertical(item.VerticalAlignment),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Foreground = ColorBrushConverter.ToBrush(item.TextColor) ?? Brushes.Black,
                        Margin = new Thickness(2)
                    };
                    if (item.Underline)
                        textBlock.TextDecorations = TextDecorations.Underline;

                    var host = new Grid();
                    host.Children.Add(textBlock);
                    element = host;
                    break;
                }
                case TemplateItemKind.Line:
                {
                    var x1 = item.Width < 0 ? item.Width : 0;
                    var y1 = item.Height < 0 ? item.Height : 0;
                    element = new Line
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = item.Width < 0 ? 0 : item.Width,
                        Y2 = item.Height < 0 ? 0 : item.Height,
                        Stroke = ColorBrushConverter.ToBrush(item.BorderColor) ?? Brushes.Black,
                        StrokeThickness = item.BorderThickness <= 0 ? 1 : item.BorderThickness,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
                    };
                    break;
                }
                case TemplateItemKind.Rectangle:
                {
                    element = new Rectangle
                    {
                        Fill = ColorBrushConverter.ToBrush(item.FillColor) ?? Brushes.Transparent,
                        Stroke = ColorBrushConverter.ToBrush(item.BorderColor) ?? Brushes.Black,
                        StrokeThickness = item.BorderThickness <= 0 ? 1 : item.BorderThickness,
                        SnapsToDevicePixels = true
                    };
                    break;
                }
                case TemplateItemKind.Ellipse:
                {
                    element = new Ellipse
                    {
                        Fill = ColorBrushConverter.ToBrush(item.FillColor) ?? Brushes.Transparent,
                        Stroke = ColorBrushConverter.ToBrush(item.BorderColor) ?? Brushes.Black,
                        StrokeThickness = item.BorderThickness <= 0 ? 1 : item.BorderThickness,
                        SnapsToDevicePixels = true
                    };
                    break;
                }
                default:
                    element = new Grid();
                    break;
            }

            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.RenderTransform = new RotateTransform(item.Rotation);
            element.Opacity = item.Opacity;
            return element;
        }

        private static VerticalAlignment MapVertical(TemplateVerticalAlignment alignment) => alignment switch
        {
            TemplateVerticalAlignment.Center => VerticalAlignment.Center,
            TemplateVerticalAlignment.Bottom => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Top
        };

        private static TextAlignment MapTextAlignment(TemplateTextAlignment alignment) => alignment switch
        {
            TemplateTextAlignment.Center => TextAlignment.Center,
            TemplateTextAlignment.Right => TextAlignment.Right,
            TemplateTextAlignment.Justify => TextAlignment.Justify,
            _ => TextAlignment.Left
        };
    }
}
