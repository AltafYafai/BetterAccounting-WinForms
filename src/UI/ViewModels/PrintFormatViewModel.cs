using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;

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
        private string _templateContent = string.Empty;
        private string _statusMessage = string.Empty;
        private ObservableCollection<string> _availableTokens = new();
        private FlowDocument _previewDocument = new();

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
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => SelectedTemplate != null);
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
            SelectedTemplate = Templates.FirstOrDefault(t => t.IsDefault) ?? Templates.FirstOrDefault();
            RefreshAvailableTokens();
            RefreshPreview();
        }

        private void RefreshAvailableTokens()
        {
            AvailableTokens = new ObservableCollection<string>(
                PrintTemplateService.GetTokens(SelectedDocumentType).Select(t => t.Token));
        }

        private void New()
        {
            TemplateName = $"New {SelectedDocumentType}";
            TemplateContent = PrintTemplateService.GetDefaultContent(SelectedDocumentType);
            SelectedTemplate = null;
            StatusMessage = "Enter a name, edit the layout, then click Save.";
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(TemplateName))
                return;

            if (SelectedTemplate is null)
            {
                var template = new PrintTemplate
                {
                    Name = TemplateName.Trim(),
                    DocumentType = SelectedDocumentType,
                    Content = TemplateContent
                };
                await _service.AddAsync(template);
                StatusMessage = $"Format '{template.Name}' was saved.";
            }
            else
            {
                SelectedTemplate.Name = TemplateName.Trim();
                SelectedTemplate.Content = TemplateContent;
                await _service.UpdateAsync(SelectedTemplate);
                StatusMessage = $"Format '{SelectedTemplate.Name}' was updated.";
            }

            await LoadTemplatesAsync();
        }

        private async Task SetDefaultAsync()
        {
            if (SelectedTemplate is null)
                return;

            await _service.SetDefaultAsync(SelectedTemplate.Id, SelectedDocumentType);
            StatusMessage = $"'{SelectedTemplate.Name}' is now the default {SelectedDocumentType} format.";
            await LoadTemplatesAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedTemplate is null)
                return;

            await _service.DeleteAsync(SelectedTemplate.Id);
            StatusMessage = "Format deleted.";
            await LoadTemplatesAsync();
        }

        private void InsertToken(object token)
        {
            var key = token?.ToString();
            if (string.IsNullOrEmpty(key))
                return;

            if (TemplateContent.Length > 0 && !TemplateContent.EndsWith("\n"))
                TemplateContent += "\n";
            TemplateContent += "{" + key + "}";
            StatusMessage = $"Inserted token '{{{key}}}'.";
        }

        private void RefreshPreview()
        {
            var fields = BuildSampleFields();
            var lines = PrintTemplateService.Render(TemplateContent, fields);
            PreviewDocument = TemplateDocumentBuilder.Build(lines);
        }

        private Dictionary<string, string> BuildSampleFields()
        {
            return new Dictionary<string, string>
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
                { "Account", "Cash" },
                { "DebitCredit", "Debit" },
                { "Amount", 1000.00m.ToString("C") },
                { "Narration", "Sample narration" },
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
                        TemplateContent = value.Content;
                    }
                    SetDefaultCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
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

        public string TemplateContent
        {
            get => _templateContent;
            set => SetProperty(ref _templateContent, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<string> AvailableTokens
        {
            get => _availableTokens;
            set => SetProperty(ref _availableTokens, value);
        }

        public FlowDocument PreviewDocument
        {
            get => _previewDocument;
            set => SetProperty(ref _previewDocument, value);
        }

        public RelayCommand NewCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommandParam InsertTokenCommand { get; }
    }
}