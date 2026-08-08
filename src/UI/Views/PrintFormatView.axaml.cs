using BetterAccounting.Core.Data.Models;
using BetterAccounting.UI.Controls;
using BetterAccounting.UI.ViewModels;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BetterAccounting.UI.Views
{
    public partial class PrintFormatView : Window
    {
        private readonly PrintFormatViewModel _vm;
        private ObservableCollection<PrintTemplateItem>? _items;

        public PrintFormatView()
        {
            InitializeComponent();

            _vm = new PrintFormatViewModel();
            DataContext = _vm;
            Designer.Substituter = _vm.SubstituteSample;

            _vm.PropertyChanged += OnViewModelPropertyChanged;
            AttachItems(_vm.Items);
            Designer.SelectionChanged += (s, e) => _vm.SelectedItem = Designer.SelectedItem;
            ApplyZoom();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PrintFormatViewModel.Items):
                    AttachItems(_vm.Items);
                    break;
                case nameof(PrintFormatViewModel.SelectedItem):
                    Designer.SelectItem(_vm.SelectedItem);
                    break;
                case nameof(PrintFormatViewModel.Zoom):
                    ApplyZoom();
                    break;
            }
        }

        private void AttachItems(ObservableCollection<PrintTemplateItem> items)
        {
            if (_items != null)
                _items.CollectionChanged -= OnItemsChanged;

            _items = items;
            if (_items != null)
            {
                _items.CollectionChanged += OnItemsChanged;
                Designer.SetItems(_items);
            }
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (PrintTemplateItem item in e.NewItems)
                    Designer.AddItem(item);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (PrintTemplateItem item in e.OldItems)
                    Designer.RemoveItem(item);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Designer.SetItems(_vm.Items);
            }
        }

        private void ApplyZoom()
        {
            var scale = _vm.Zoom / 100.0;
            CanvasZoomHost.RenderTransform = new ScaleTransform(scale, scale);
        }
    }
}
