using BetterAccounting.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BetterAccounting.UI.Controls
{
    public class DesignerCanvas : Canvas
    {
        private readonly Dictionary<PrintTemplateItem, DesignerItem> _map = new();

        public event EventHandler? SelectionChanged;

        public PrintTemplateItem? SelectedItem { get; private set; }

        public Func<string, string>? Substituter { get; set; }

        public DesignerCanvas()
        {
            Background = Brushes.White;
            Focusable = true;
            PreviewMouseLeftButtonDown += OnCanvasMouseDown;
        }

        public void SetItems(IEnumerable<PrintTemplateItem> items)
        {
            Children.Clear();
            _map.Clear();
            SelectedItem = null;

            foreach (var item in items)
                AddItem(item);

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddItem(PrintTemplateItem item)
        {
            var designerItem = new DesignerItem(item, this, Substituter);
            _map[item] = designerItem;
            Children.Add(designerItem);
        }

        public void RemoveItem(PrintTemplateItem item)
        {
            if (!_map.TryGetValue(item, out var designerItem))
                return;

            Children.Remove(designerItem);
            _map.Remove(item);
            if (ReferenceEquals(SelectedItem, item))
                SelectItem(null);
        }

        public void SelectItem(PrintTemplateItem? item)
        {
            if (ReferenceEquals(SelectedItem, item))
                return;

            SelectedItem = item;
            foreach (var kvp in _map)
                kvp.Value.RefreshSelectionVisual();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RefreshZOrder()
        {
            foreach (var kvp in _map)
                kvp.Value.RefreshZIndex();
        }

        public void RefreshAll()
        {
            foreach (var kvp in _map)
            {
                kvp.Value.RefreshSelectionVisual();
                kvp.Value.RefreshZIndex();
            }
        }

        private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            if (e.OriginalSource is DesignerCanvas)
                SelectItem(null);
        }
    }
}
