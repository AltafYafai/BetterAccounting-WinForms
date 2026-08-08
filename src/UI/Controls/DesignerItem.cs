using BetterAccounting.Core.Data.Models;
using BetterAccounting.UI.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BetterAccounting.UI.Controls
{
    public class DesignerItem : ContentControl
    {
        private enum ResizeEdge { None, N, S, E, W, NE, NW, SE, SW }

        private const double ThumbSize = 7;
        private const double MinShapeSize = 8;

        private readonly PrintTemplateItem _item;
        private readonly DesignerCanvas _designerCanvas;
        private readonly Func<string, string>? _substitute;

        private Grid _root = new();
        private Grid _shapeHost = new();
        private Control _shape = null!;
        private Thumb _moveThumb;
        private Border _selectionBorder = new();
        private readonly List<Thumb> _resizeThumbs = new();
        private bool _isSelected;

        private double _startX;
        private double _startY;
        private double _startWidth;
        private double _startHeight;

        public PrintTemplateItem Item => _item;

        public DesignerItem(PrintTemplateItem item, DesignerCanvas designerCanvas, Func<string, string>? substitute)
        {
            _item = item;
            _designerCanvas = designerCanvas;
            _substitute = substitute;

            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
            Focusable = false;

            _moveThumb = new Thumb
            {
                Cursor = new Cursor(StandardCursorType.SizeAll),
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            _moveThumb.DragDelta += OnMoveDragDelta;
            _moveThumb.DragStarted += (s, e) => Select();

            _selectionBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(200, 51, 122, 183)),
                BorderThickness = new Thickness(1.5),
                IsHitTestVisible = false,
                IsVisible = false
            };

            _root = new Grid();
            _shapeHost = new Grid();
            _root.Children.Add(_shapeHost);
            _root.Children.Add(_selectionBorder);
            _root.Children.Add(_moveThumb);
            Content = _root;

            BuildShape();
            BuildResizeHandles();

            AddHandler(PointerPressedEvent, (s, e) => Select(), RoutingStrategies.Tunnel);

            _item.PropertyChanged += OnItemPropertyChanged;
            UpdatePositionAndSize();
            RefreshSelectionVisual();
            RefreshZIndex();
        }

        private void BuildShape()
        {
            _shapeHost.Children.Clear();
            _shape = TemplateItemShapeFactory.Create(_item, _substitute);
            _shape.HorizontalAlignment = HorizontalAlignment.Stretch;
            _shape.VerticalAlignment = VerticalAlignment.Stretch;
            _shapeHost.Children.Add(_shape);
        }

        private void BuildResizeHandles()
        {
            var overlay = new Grid
            {
                Margin = new Thickness(-ThumbSize / 2),
                IsHitTestVisible = true
            };
            overlay.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ThumbSize) });
            overlay.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlay.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ThumbSize) });
            overlay.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ThumbSize) });
            overlay.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            overlay.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ThumbSize) });

            AddThumb(overlay, 0, 0, ResizeEdge.NW, new Cursor(StandardCursorType.TopLeftCorner));
            AddThumb(overlay, 0, 1, ResizeEdge.N, new Cursor(StandardCursorType.SizeNorthSouth));
            AddThumb(overlay, 0, 2, ResizeEdge.NE, new Cursor(StandardCursorType.TopRightCorner));
            AddThumb(overlay, 1, 0, ResizeEdge.W, new Cursor(StandardCursorType.SizeWestEast));
            AddThumb(overlay, 1, 2, ResizeEdge.E, new Cursor(StandardCursorType.SizeWestEast));
            AddThumb(overlay, 2, 0, ResizeEdge.SW, new Cursor(StandardCursorType.TopRightCorner));
            AddThumb(overlay, 2, 1, ResizeEdge.S, new Cursor(StandardCursorType.SizeNorthSouth));
            AddThumb(overlay, 2, 2, ResizeEdge.SE, new Cursor(StandardCursorType.TopLeftCorner));

            _root.Children.Add(overlay);
        }

        private void AddThumb(Grid overlay, int row, int col, ResizeEdge edge, Cursor cursor)
        {
            var thumb = new Thumb
            {
                Width = ThumbSize,
                Height = ThumbSize,
                Cursor = cursor,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 122, 183)),
                BorderThickness = new Thickness(1)
            };
            Grid.SetRow(thumb, row);
            Grid.SetColumn(thumb, col);
            thumb.DragStarted += (s, e) =>
            {
                _startX = _item.X;
                _startY = _item.Y;
                _startWidth = _item.Width;
                _startHeight = _item.Height;
                Select();
            };
            thumb.DragDelta += (s, e) => OnResizeDragDelta(edge, e);
            overlay.Children.Add(thumb);
            _resizeThumbs.Add(thumb);
        }

        private void Select()
        {
            if (!_isSelected)
                _designerCanvas.SelectItem(_item);
        }

        private void OnMoveDragDelta(object sender, VectorEventArgs e)
        {
            _item.X += e.Vector.X;
            _item.Y += e.Vector.Y;
        }

        private void OnResizeDragDelta(ResizeEdge edge, VectorEventArgs e)
        {
            var x = _startX;
            var y = _startY;
            var w = _startWidth;
            var h = _startHeight;
            var hChange = e.Vector.X;
            var vChange = e.Vector.Y;

            switch (edge)
            {
                case ResizeEdge.E: w += hChange; break;
                case ResizeEdge.S: h += vChange; break;
                case ResizeEdge.SE: w += hChange; h += vChange; break;
                case ResizeEdge.W: x += hChange; w -= hChange; break;
                case ResizeEdge.N: y += vChange; h -= vChange; break;
                case ResizeEdge.NW: x += hChange; y += vChange; w -= hChange; h -= vChange; break;
                case ResizeEdge.NE: y += vChange; w += hChange; h -= vChange; break;
                case ResizeEdge.SW: x += hChange; w -= hChange; h += vChange; break;
            }

            var min = _item.Kind == TemplateItemKind.Line ? 0 : MinShapeSize;
            if (w < min || h < min)
                return;

            _item.X = x;
            _item.Y = y;
            _item.Width = w;
            _item.Height = h;
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PrintTemplateItem.X):
                case nameof(PrintTemplateItem.Y):
                case nameof(PrintTemplateItem.Width):
                case nameof(PrintTemplateItem.Height):
                    UpdatePositionAndSize();
                    break;
                case nameof(PrintTemplateItem.ZIndex):
                    RefreshZIndex();
                    break;
                case nameof(PrintTemplateItem.Rotation):
                case nameof(PrintTemplateItem.Text):
                case nameof(PrintTemplateItem.FontFamily):
                case nameof(PrintTemplateItem.FontSize):
                case nameof(PrintTemplateItem.Bold):
                case nameof(PrintTemplateItem.Italic):
                case nameof(PrintTemplateItem.Underline):
                case nameof(PrintTemplateItem.TextColor):
                case nameof(PrintTemplateItem.FillColor):
                case nameof(PrintTemplateItem.BorderColor):
                case nameof(PrintTemplateItem.BorderThickness):
                case nameof(PrintTemplateItem.TextAlignment):
                case nameof(PrintTemplateItem.VerticalAlignment):
                case nameof(PrintTemplateItem.Opacity):
                    BuildShape();
                    break;
            }
        }

        private void UpdatePositionAndSize()
        {
            var (x, y, w, h) = TemplateItemShapeFactory.Normalize(_item);
            if (_item.Kind == TemplateItemKind.Line)
            {
                w = Math.Max(w, 8);
                h = Math.Max(h, 8);
            }
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
            Width = w;
            Height = h;
        }

        public void RefreshSelectionVisual()
        {
            _isSelected = _designerCanvas.SelectedItem == _item;
            _selectionBorder.IsVisible = _isSelected;
            foreach (var thumb in _resizeThumbs)
                thumb.IsVisible = _isSelected && _item.Rotation == 0;
            _moveThumb.Cursor = _isSelected ? new Cursor(StandardCursorType.SizeAll) : new Cursor(StandardCursorType.Hand);
            RefreshZIndex();
        }

        public void RefreshZIndex()
        {
            ZIndex = _item.ZIndex + (_isSelected ? 10000 : 0);
        }
    }
}
