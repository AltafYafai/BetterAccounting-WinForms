using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class PrintPreviewViewModel : ViewModelBase
    {
        private readonly LedgerEntry _entry;
        private readonly CompanyProfile? _company;
        private readonly PrintTemplate? _template;
        private readonly PrintTemplateLayout? _layout;
        private string _selectedCopy = "Original";

        public PrintPreviewViewModel(LedgerEntry entry)
        {
            _entry = entry;
            _company = LoadCompanyProfile();
            _template = LoadTemplate();
            _layout = _template != null
                ? PrintTemplateSerializer.TryDeserialize(_template.Content)
                : null;

            if (_layout != null)
            {
                FixedDocument = BuildLayoutDocument("ORIGINAL");
                UsesFixedDocument = true;
            }
            else
            {
                Document = VoucherDocumentBuilder.Build(entry, _company, "ORIGINAL", _template);
                UsesFixedDocument = false;
            }

            PrintCommand = new RelayCommand(Print);
        }

        private static CompanyProfile? LoadCompanyProfile()
        {
            var dbPath = BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();

            try
            {
                using var context = new SQLiteContext(dbPath);
                var repository = new CompanyProfileRepository(context.Connection);
                return repository.GetAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Load company profile for print preview", ex);
                return null;
            }
        }

        private static PrintTemplate? LoadTemplate()
        {
            var dbPath = BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();

            try
            {
                using var context = new SQLiteContext(dbPath);
                var repository = new PrintTemplateRepository(context.Connection);
                return repository.GetDefaultAsync(DocumentType.Invoice).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Load print template for print preview", ex);
                return null;
            }
        }

        private FixedDocument BuildLayoutDocument(string copyLabel)
        {
            var fields = VoucherDocumentBuilder.BuildFields(_entry, _company);
            fields["CopyLabel"] = copyLabel;
            return PrintLayoutRenderer.BuildFixedDocument(_layout!, fields);
        }

        private void Print()
        {
            try
            {
                var count = CopyCount;
                var dialog = new PrintDialog();
                if (dialog.ShowDialog() != true)
                    return;

                for (var i = 1; i <= count; i++)
                {
                    var label = i == 1 ? "ORIGINAL" : i == 2 ? "DUPLICATE" : "TRIPLICATE";
                    if (UsesFixedDocument && _layout != null)
                    {
                        var document = BuildLayoutDocument(label);
                        dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator,
                            $"Voucher {_entry.VoucherNo} - {label}");
                    }
                    else
                    {
                        var document = VoucherDocumentBuilder.Build(_entry, _company, label, _template);
                        dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator,
                            $"Voucher {_entry.VoucherNo} - {label}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Show($"Print voucher '{_entry.VoucherNo}'", ex);
            }
        }

        private int CopyCount => SelectedCopy switch
        {
            "Duplicate" => 2,
            "Triplicate" => 3,
            _ => 1
        };

        public string[] CopyOptions { get; } = { "Original", "Duplicate", "Triplicate" };
        public FlowDocument? Document { get; }
        public FixedDocument? FixedDocument { get; }
        public bool UsesFixedDocument { get; }

        public string SelectedCopy
        {
            get => _selectedCopy;
            set => SetProperty(ref _selectedCopy, value);
        }

        public ICommand PrintCommand { get; }
    }
}
