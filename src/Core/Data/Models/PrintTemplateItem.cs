using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BetterAccounting.Core.Data.Models
{
    public enum TemplateItemKind
    {
        Text,
        Line,
        Rectangle,
        Ellipse
    }

    public enum TemplateTextAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    public enum TemplateVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public class PrintTemplateItem : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString("N");
        private TemplateItemKind _kind = TemplateItemKind.Text;
        private double _x;
        private double _y;
        private double _width = 120;
        private double _height = 24;
        private double _rotation;
        private string _text = "Text";
        private string _fontFamily = "Segoe UI";
        private double _fontSize = 12;
        private bool _bold;
        private bool _italic;
        private bool _underline;
        private string? _textColor = "Black";
        private string? _fillColor = "Transparent";
        private string? _borderColor = "Black";
        private double _borderThickness = 1;
        private TemplateTextAlignment _textAlignment = TemplateTextAlignment.Left;
        private TemplateVerticalAlignment _verticalAlignment = TemplateVerticalAlignment.Top;
        private double _opacity = 1;
        private int _zIndex;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public TemplateItemKind Kind
        {
            get => _kind;
            set => SetProperty(ref _kind, value);
        }

        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public double Rotation
        {
            get => _rotation;
            set => SetProperty(ref _rotation, value);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public string FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public bool Bold
        {
            get => _bold;
            set => SetProperty(ref _bold, value);
        }

        public bool Italic
        {
            get => _italic;
            set => SetProperty(ref _italic, value);
        }

        public bool Underline
        {
            get => _underline;
            set => SetProperty(ref _underline, value);
        }

        public string? TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        public string? FillColor
        {
            get => _fillColor;
            set => SetProperty(ref _fillColor, value);
        }

        public string? BorderColor
        {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        public double BorderThickness
        {
            get => _borderThickness;
            set => SetProperty(ref _borderThickness, value);
        }

        public TemplateTextAlignment TextAlignment
        {
            get => _textAlignment;
            set => SetProperty(ref _textAlignment, value);
        }

        public TemplateVerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set => SetProperty(ref _verticalAlignment, value);
        }

        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        public int ZIndex
        {
            get => _zIndex;
            set => SetProperty(ref _zIndex, value);
        }

        public PrintTemplateItem Clone()
        {
            return new PrintTemplateItem
            {
                Id = Guid.NewGuid().ToString("N"),
                Kind = Kind,
                X = X + 12,
                Y = Y + 12,
                Width = Width,
                Height = Height,
                Rotation = Rotation,
                Text = Text,
                FontFamily = FontFamily,
                FontSize = FontSize,
                Bold = Bold,
                Italic = Italic,
                Underline = Underline,
                TextColor = TextColor,
                FillColor = FillColor,
                BorderColor = BorderColor,
                BorderThickness = BorderThickness,
                TextAlignment = TextAlignment,
                VerticalAlignment = VerticalAlignment,
                Opacity = Opacity,
                ZIndex = ZIndex
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value))
                return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
