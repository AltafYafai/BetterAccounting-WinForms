using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
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
        private string _selectedCopy = "Original";

        public PrintPreviewViewModel(LedgerEntry entry)
        {
            _entry = entry;
            _company = LoadCompanyProfile();
            _template = LoadTemplate();
            Document = VoucherDocumentBuilder.Build(entry, _company, "ORIGINAL", _template);
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
            catch
            {
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
            catch
            {
                return null;
            }
        }

        private void Print()
        {
            var count = CopyCount;
            var dialog = new PrintDialog();
            if (dialog.ShowDialog() != true)
                return;

            for (var i = 1; i <= count; i++)
            {
                var label = i == 1 ? "ORIGINAL" : i == 2 ? "DUPLICATE" : "TRIPLICATE";
                var document = VoucherDocumentBuilder.Build(_entry, _company, label, _template);
                dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator,
                    $"Voucher {_entry.VoucherNo} - {label}");
            }
        }

        private int CopyCount => SelectedCopy switch
        {
            "Duplicate" => 2,
            "Triplicate" => 3,
            _ => 1
        };

        public string[] CopyOptions { get; } = { "Original", "Duplicate", "Triplicate" };
        public FlowDocument Document { get; }

        public string SelectedCopy
        {
            get => _selectedCopy;
            set => SetProperty(ref _selectedCopy, value);
        }

        public ICommand PrintCommand { get; }
    }
}