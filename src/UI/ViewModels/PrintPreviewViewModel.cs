using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using BetterAccounting.UI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            Document = _layout != null
                ? PrintLayoutRenderer.BuildLayoutDocument(_layout, BuildFields("ORIGINAL"))
                : VoucherDocumentBuilder.Build(entry, _company, "ORIGINAL", _template);

            PrintCommand = new RelayCommand(async () => await PrintAsync());
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

        private Dictionary<string, string> BuildFields(string copyLabel)
        {
            var fields = VoucherDocumentBuilder.BuildFields(_entry, _company);
            fields["CopyLabel"] = copyLabel;
            return fields;
        }

        private async Task PrintAsync()
        {
            try
            {
                var count = CopyCount;
                var model = new PrintDocumentModel();

                for (var i = 1; i <= count; i++)
                {
                    var label = i == 1 ? "ORIGINAL" : i == 2 ? "DUPLICATE" : "TRIPLICATE";
                    if (_layout != null)
                    {
                        foreach (var page in PrintLayoutRenderer.BuildLayoutDocument(_layout, BuildFields(label)).Pages)
                            model.Pages.Add(page);
                    }
                    else
                    {
                        foreach (var page in VoucherDocumentBuilder.Build(_entry, _company, label, _template).Pages)
                            model.Pages.Add(page);
                    }
                }

                var path = await PdfExportService.ExportAsync(model, $"Voucher_{_entry.VoucherNo}");
                if (path != null)
                    PdfExportService.OpenDocument(path);
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
        public PrintDocumentModel Document { get; }

        public string SelectedCopy
        {
            get => _selectedCopy;
            set => SetProperty(ref _selectedCopy, value);
        }

        public ICommand PrintCommand { get; }
    }
}
