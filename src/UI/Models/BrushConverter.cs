using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BetterAccounting.UI.Models
{
    public class BrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Brush)(ColorBrushConverter.ToBrush(value as string) ?? Brushes.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
