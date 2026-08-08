using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BetterAccounting.UI.ViewModels
{
    public class PrintFormatViewModel : ViewModelBase
    {
        private readonly SQLiteContext _context;
        private readonly PrintTemplateService _service;
        private readonly CompanyProfileRepository _companyRepository;
        private CompanyProfile? _company;

        private ObservableCollection<DocumentType> _documentTypes = new();
        private DocumentType _selectedDocumentType = DocumentType.Invoice;
        private ObservableCollection<PrintTemplate> _templates = new();
        private PrintTemplate? _selectedTemplate;
        private string _templateName = string.Empty;
        private string _statusMessage = string.Empty;

        private PrintTemplateLayout _layout = PrintTemplateLayout.CreateDefault();
        private ObservableCollection<PrintTemplateItem> _items = new();
        private PrintTemplateItem? _selectedItem;
        private string _selectedPageSize = "A4";
        private bool _isLandscape;
        private double _zoom = 100;
        private ObservableCollection<string> _availableTokens = new();

        public PrintFormatViewModel()
        {
            _context = new SQLiteContext(GetDatabasePath());
            var connection = _context.Connection;
            _service = new PrintTemplateService(new PrintTemplateRepository(connection));
            _companyRepository = new CompanyProfileRepository(connection);

            DocumentTypes = new ObservableCollection<DocumentType>(Enum.GetValues<DocumentType>());

            NewCommand = new RelayCommand(New);
            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !string.IsNullOrWhiteSpace(TemplateName));
            SetDefaultCommand = new RelayCommand(async () => await SetDefaultAsync(), () => SelectedTemplate != null);
            DeleteTemplateCommand = new RelayCommand(async () => await DeleteTemplateAsync(), () => SelectedTemplate != null);

            AddTextCommand = new RelayCommand(() => AddItem(TemplateItemKind.Text));
            AddLineCommand = new RelayCommand(() => AddItem(TemplateItemKind.Line));
            AddRectangleCommand = new RelayCommand(() => AddItem(TemplateItemKind.Rectangle));
            AddEllipseCommand = new RelayCommand(() => AddItem(TemplateItemKind.Ellipse));
            DeleteItemCommand = new RelayCommand(DeleteItem, () => SelectedItem != null);
            DuplicateItemCommand = new RelayCommand(DuplicateItem, () => SelectedItem != null);
            BringToFrontCommand = new RelayCommand(BringToFront, () => SelectedItem != null);
            SendToBackCommand = new RelayCommand(SendToBack, () => SelectedItem != null);
            AlignLeftCommand = new RelayCommand(() => AlignHorizontal(false, 0), () => SelectedItem != null);
            AlignCenterHCommand = new RelayCommand(() => AlignHorizontal(true, 1), () => SelectedItem != null);
            AlignRightCommand = new RelayCommand(() => AlignHorizontal(false, 1), () => SelectedItem != null);
            AlignTopCommand = new RelayCommand(() => AlignVertical(false, 0), () => SelectedItem != null);
            AlignMiddleCommand = new RelayCommand(() => AlignVertical(true, 1), () => SelectedItem != null);
            AlignBottomCommand = new RelayCommand(() => AlignVertical(false, 1), () => SelectedItem != null);

            InsertTokenCommand = new RelayCommandParam(InsertToken);

            _ = LoadAsync();
        }

        private static string GetDatabasePath()
        {
            return BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();
        }

        private async Task LoadAsync()
        {
            try
            {
                _company = await _companyRepository.GetAsync();
            }
            catch
            {
            }

            await _service.EnsureDefaultsAsync();
            await LoadTemplatesAsync();
        }

        private async Task LoadTemplatesAsync()
        {
            var list = await _service.GetTemplatesAsync(SelectedDocumentType);
            Templates = new ObservableCollection<PrintTemplate>(list);
            RefreshAvailableTokens();
            UpdateSampleFields();
            SelectedTemplate = Templates.FirstOrDefault(t => t.IsDefault) ?? Templates.FirstOrDefault();
        }

        private void RefreshAvailableTokens()
        {
            AvailableTokens = new ObservableCollection<string>(
                PrintTemplateService.GetTokens(SelectedDocumentType).Select(t => t.Token));
        }

        private void New()
        {
            TemplateName = $"New {SelectedDocumentType}";
            LoadLayout(DefaultLayoutFactory.Create(SelectedDocumentType));
            SelectedTemplate = null;
            StatusMessage = "Enter a name, arrange the layout, then click Save.";
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(TemplateName))
                return;

            var content = PrintTemplateSerializer.Serialize(Layout);
            int? savedId;
            if (SelectedTemplate is null)
            {
                var template = new PrintTemplate
                {
                    Name = TemplateName.Trim(),
                    DocumentType = SelectedDocumentType,
                    Content = content
                };
                await _service.AddAsync(template);
                savedId = template.Id;
                StatusMessage = $"Format '{template.Name}' was saved.";
            }
            else
            {
                SelectedTemplate.Name = TemplateName.Trim();
                SelectedTemplate.Content = content;
                await _service.UpdateAsync(SelectedTemplate);
                savedId = SelectedTemplate.Id;
                StatusMessage = $"Format '{SelectedTemplate.Name}' was updated.";
            }

            await LoadTemplatesAsync();
            if (savedId.HasValue)
            {
                var saved = Templates.FirstOrDefault(t => t.Id == savedId.Value);
                if (saved != null)
                    SelectedTemplate = saved;
            }
        }

        private async Task SetDefaultAsync()
        {
            if (SelectedTemplate is null)
                return;

            await _service.SetDefaultAsync(SelectedTemplate.Id, SelectedDocumentType);
            StatusMessage = $"'{SelectedTemplate.Name}' is now the default {SelectedDocumentType} format.";
            await LoadTemplatesAsync();
        }

        private async Task DeleteTemplateAsync()
        {
            if (SelectedTemplate is null)
                return;

            await _service.DeleteAsync(SelectedTemplate.Id);
            StatusMessage = "Format deleted.";
            SelectedTemplate = null;
            await LoadTemplatesAsync();
        }

        private void InsertToken(object token)
        {
            var key = token?.ToString();
            if (string.IsNullOrEmpty(key) || SelectedItem is not { Kind: TemplateItemKind.Text } textItem)
                return;

            textItem.Text = string.IsNullOrWhiteSpace(textItem.Text)
                ? "{" + key + "}"
                : textItem.Text + " {" + key + "}";
            StatusMessage = $"Inserted token '{{{key}}}'.";
        }

        private void AddItem(TemplateItemKind kind)
        {
            var (w, h) = kind switch
            {
                TemplateItemKind.Text => (200.0, 28.0),
                TemplateItemKind.Line => (300.0, 0.0),
                TemplateItemKind.Rectangle => (200.0, 120.0),
                TemplateItemKind.Ellipse => (160.0, 120.0),
                _ => (100.0, 40.0)
            };

            var item = new PrintTemplateItem
            {
                Kind = kind,
                X = (PageWidth - w) / 2,
                Y = (PageHeight - h) / 2,
                Width = w,
                Height = h,
                Text = kind == TemplateItemKind.Text ? "Text" : string.Empty,
                ZIndex = NextZIndex()
            };
            Items.Add(item);
            SelectedItem = item;
            StatusMessage = $"Added {kind}.";
        }

        private void DeleteItem()
        {
            if (SelectedItem is null)
                return;
            Items.Remove(SelectedItem);
            SelectedItem = null;
        }

        private void DuplicateItem()
        {
            if (SelectedItem is null)
                return;
            var clone = SelectedItem.Clone();
            clone.ZIndex = NextZIndex();
            Items.Add(clone);
            SelectedItem = clone;
        }

        private int NextZIndex()
        {
            return Items.Count == 0 ? 0 : Items.Max(i => i.ZIndex) + 1;
        }

        private void BringToFront()
        {
            if (SelectedItem is null)
                return;
            var max = Items.Count == 0 ? 0 : Items.Max(i => i.ZIndex);
            SelectedItem.ZIndex = max + 1;
        }

        private void SendToBack()
        {
            if (SelectedItem is null)
                return;
            var min = Items.Count == 0 ? 0 : Items.Min(i => i.ZIndex);
            SelectedItem.ZIndex = min - 1;
        }

        private void AlignHorizontal(bool center, int edge)
        {
            if (SelectedItem is null)
                return;
            var item = SelectedItem;
            item.X = center
                ? (PageWidth - item.Width) / 2
                : edge == 0 ? 40 : PageWidth - item.Width - 40;
        }

        private void AlignVertical(bool middle, int edge)
        {
            if (SelectedItem is null)
                return;
            var item = SelectedItem;
            item.Y = middle
                ? (PageHeight - item.Height) / 2
                : edge == 0 ? 40 : PageHeight - item.Height - 40;
        }

        private void LoadLayout(PrintTemplateLayout layout)
        {
            foreach (var item in layout.Items)
            {
                item.TextColor ??= "Black";
                item.FillColor ??= "Transparent";
                item.BorderColor ??= "Black";
                item.Rotation = ((item.Rotation % 360) + 360) % 360;
            }

            Layout = layout;
            _items = new ObservableCollection<PrintTemplateItem>(layout.Items);
            SelectedItem = null;

            SelectedPageSize = layout.PageName;
            IsLandscape = layout.Landscape;
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PageWidth));
            OnPropertyChanged(nameof(PageHeight));
        }

        private void UpdateSampleFields()
        {
            SampleFields = new Dictionary<string, string>
            {
                { "CompanyName", _company?.CompanyName ?? "Your Company Pvt Ltd" },
                { "Gstin", _company?.Gstin ?? "27ABCDE1234F1Z5" },
                { "Address", _company?.Address ?? "1, Main Road" },
                { "City", _company?.City ?? "Pune" },
                { "State", _company?.State ?? "Maharashtra" },
                { "PinCode", _company?.PinCode ?? "411001" },
                { "Phone", _company?.Phone ?? "+91-9876543210" },
                { "Email", _company?.Email ?? "accounts@example.com" },
                { "VoucherType", "Debit Note" },
                { "VoucherNo", "V-1001" },
                { "Date", DateTime.Today.ToShortDateString() },
                { "CopyLabel", "ORIGINAL" },
                { "Account", "Cash" },
                { "DebitCredit", "Debit" },
                { "Amount", 1000.00m.ToString("C") },
                { "Narration", "Sample narration for this voucher" },
                { "Transporter", "Transporter Co." },
                { "AccountName", "Cash" },
                { "OpeningBalance", 0.00m.ToString("C") },
                { "ClosingBalance", 500.00m.ToString("C") },
                { "FromDate", DateTime.Today.AddMonths(-1).ToShortDateString() },
                { "ToDate", DateTime.Today.ToShortDateString() },
                { "ReportTitle", "Trial Balance" },
                { "Version", "1.0.0" },
                { "PrintedBy", Environment.UserName },
                { "CreatedDate", DateTime.Now.ToString("g") }
            };
        }

        public string SubstituteSample(string text)
        {
            return PrintTemplateService.Substitute(text, SampleFields);
        }

        public Dictionary<string, string> SampleFields { get; private set; } = new();

        public ObservableCollection<DocumentType> DocumentTypes
        {
            get => _documentTypes;
            set => SetProperty(ref _documentTypes, value);
        }

        public DocumentType SelectedDocumentType
        {
            get => _selectedDocumentType;
            set
            {
                if (SetProperty(ref _selectedDocumentType, value))
                    _ = LoadTemplatesAsync();
            }
        }

        public ObservableCollection<PrintTemplate> Templates
        {
            get => _templates;
            set => SetProperty(ref _templates, value);
        }

        public PrintTemplate? SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                if (_selectedTemplate != value)
                {
                    _selectedTemplate = value;
                    OnPropertyChanged();
                    if (value is not null)
                    {
                        TemplateName = value.Name;
                        var layout = PrintTemplateSerializer.TryDeserialize(value.Content)
                            ?? LegacyTemplateConverter.Convert(value.Content);
                        LoadLayout(layout);
                        StatusMessage = value.IsDefault
                            ? $"'{value.Name}' is the default {SelectedDocumentType} format."
                            : $"Editing '{value.Name}'.";
                    }
                    SetDefaultCommand.RaiseCanExecuteChanged();
                    DeleteTemplateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string TemplateName
        {
            get => _templateName;
            set
            {
                if (SetProperty(ref _templateName, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public PrintTemplateLayout Layout
        {
            get => _layout;
            set => SetProperty(ref _layout, value);
        }

        public ObservableCollection<PrintTemplateItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public PrintTemplateItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsTextItemSelected));
                    OnPropertyChanged(nameof(IsShapeItemSelected));

                    DeleteItemCommand.RaiseCanExecuteChanged();
                    DuplicateItemCommand.RaiseCanExecuteChanged();
                    BringToFrontCommand.RaiseCanExecuteChanged();
                    SendToBackCommand.RaiseCanExecuteChanged();
                    AlignLeftCommand.RaiseCanExecuteChanged();
                    AlignCenterHCommand.RaiseCanExecuteChanged();
                    AlignRightCommand.RaiseCanExecuteChanged();
                    AlignTopCommand.RaiseCanExecuteChanged();
                    AlignMiddleCommand.RaiseCanExecuteChanged();
                    AlignBottomCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsTextItemSelected => SelectedItem?.Kind == TemplateItemKind.Text;
        public bool IsShapeItemSelected => SelectedItem != null && SelectedItem.Kind != TemplateItemKind.Text;

        public string[] PageSizes { get; } = { "A5", "A4", "A3", "Letter", "Legal" };

        public string SelectedPageSize
        {
            get => _selectedPageSize;
            set
            {
                if (SetProperty(ref _selectedPageSize, value))
                    ApplyPageSize();
            }
        }

        public bool IsLandscape
        {
            get => _isLandscape;
            set
            {
                if (SetProperty(ref _isLandscape, value))
                    ApplyPageSize();
            }
        }

        public double PageWidth => Layout.PageWidth;
        public double PageHeight => Layout.PageHeight;

        private void ApplyPageSize()
        {
            var (width, height, name) = PrintTemplateLayout.GetPageSize(SelectedPageSize, IsLandscape);
            Layout.PageWidth = width;
            Layout.PageHeight = height;
            Layout.PageName = name;
            Layout.Landscape = IsLandscape;
            OnPropertyChanged(nameof(PageWidth));
            OnPropertyChanged(nameof(PageHeight));
        }

        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        public ObservableCollection<string> AvailableTokens
        {
            get => _availableTokens;
            set => SetProperty(ref _availableTokens, value);
        }

        public string[] FontFamilies { get; } =
        {
            "Segoe UI", "Arial", "Calibri", "Cambria", "Consolas", "Courier New",
            "Georgia", "Times New Roman", "Tahoma", "Trebuchet MS", "Verdana"
        };

        public string[] ColorChoices { get; } =
        {
            "Transparent", "Black", "White", "Red", "DarkRed", "Orange", "Yellow", "Green",
            "DarkGreen", "Blue", "DarkBlue", "Purple", "Magenta", "Cyan", "Brown", "Gray",
            "DarkGray", "LightGray", "#333333", "#666666", "#999999", "#CCCCCC", "#EEEEEE", "#F4F4F4"
        };

        public TemplateTextAlignment[] TextAlignments { get; } = Enum.GetValues<TemplateTextAlignment>();
        public TemplateVerticalAlignment[] VerticalAlignments { get; } = Enum.GetValues<TemplateVerticalAlignment>();

        public RelayCommand NewCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public RelayCommand DeleteTemplateCommand { get; }

        public RelayCommand AddTextCommand { get; }
        public RelayCommand AddLineCommand { get; }
        public RelayCommand AddRectangleCommand { get; }
        public RelayCommand AddEllipseCommand { get; }
        public RelayCommand DeleteItemCommand { get; }
        public RelayCommand DuplicateItemCommand { get; }
        public RelayCommand BringToFrontCommand { get; }
        public RelayCommand SendToBackCommand { get; }
        public RelayCommand AlignLeftCommand { get; }
        public RelayCommand AlignCenterHCommand { get; }
        public RelayCommand AlignRightCommand { get; }
        public RelayCommand AlignTopCommand { get; }
        public RelayCommand AlignMiddleCommand { get; }
        public RelayCommand AlignBottomCommand { get; }
        public RelayCommandParam InsertTokenCommand { get; }
    }
}
