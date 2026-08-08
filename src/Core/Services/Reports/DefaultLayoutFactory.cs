using BetterAccounting.Core.Data.Models;

namespace BetterAccounting.Core.Services.Reports
{
    public static class DefaultLayoutFactory
    {
        public static PrintTemplateLayout Create(DocumentType type)
        {
            var layout = PrintTemplateLayout.CreateDefault();
            layout.Items.Clear();

            switch (type)
            {
                case DocumentType.Invoice:
                    BuildInvoice(layout);
                    break;
                case DocumentType.Ledger:
                    BuildLedger(layout);
                    break;
                case DocumentType.Cover:
                    BuildCover(layout);
                    break;
                case DocumentType.Report:
                    BuildReport(layout);
                    break;
            }

            return layout;
        }

        private static void BuildInvoice(PrintTemplateLayout layout)
        {
            Text(layout, "{CompanyName}", 60, 40, 674, 40, 24, bold: true, align: TemplateTextAlignment.Center);
            Text(layout, "GSTIN: {Gstin}", 60, 86, 674, 18, 11, align: TemplateTextAlignment.Center);
            Text(layout, "{Address}", 60, 106, 674, 18, 11, align: TemplateTextAlignment.Center);
            Text(layout, "{City}, {State} - {PinCode}", 60, 126, 674, 18, 11, align: TemplateTextAlignment.Center);
            Text(layout, "{Phone}   |   {Email}", 60, 146, 674, 18, 11, align: TemplateTextAlignment.Center);

            Line(layout, 60, 186, 734, 186, 2);

            Text(layout, "VOUCHER", 60, 200, 674, 34, 22, bold: true, align: TemplateTextAlignment.Center);

            Text(layout, "Voucher Type  : {VoucherType}", 80, 250, 320, 22, 12, bold: true);
            Text(layout, "Voucher No    : {VoucherNo}", 414, 250, 320, 22, 12, bold: true);
            Text(layout, "Date          : {Date}", 414, 274, 320, 22, 12);
            Text(layout, "Account       : {Account}", 80, 274, 320, 22, 12);
            Text(layout, "Debit/Credit  : {DebitCredit}", 80, 298, 320, 22, 12);
            Text(layout, "Amount        : {Amount}", 80, 322, 320, 22, 12, bold: true);

            Rectangle(layout, 80, 340, 320, 40, border: "222222", borderThickness: 1.5, fill: "F4F4F4");
            Text(layout, "Amount (in words)", 88, 346, 300, 16, 10, color: "666666");
            Text(layout, "{Amount}", 88, 362, 300, 18, 12, bold: true);

            Text(layout, "Narration  : {Narration}", 80, 400, 614, 80, 12);

            Text(layout, "Transporter : {Transporter}", 80, 500, 614, 20, 11);
            Line(layout, 60, 600, 734, 600, 1);
            Text(layout, "Printed on  : {CreatedDate}", 80, 612, 614, 20, 11);

            Text(layout, "{CompanyName}", 397, 460, 300, 120, 34, color: "DDDDDD", opacity: 0.5, rotation: 315,
                align: TemplateTextAlignment.Center);
        }

        private static void BuildLedger(PrintTemplateLayout layout)
        {
            Text(layout, "{CompanyName}", 60, 40, 674, 34, 22, bold: true, align: TemplateTextAlignment.Center);
            Text(layout, "GSTIN: {Gstin}", 60, 78, 674, 18, 11, align: TemplateTextAlignment.Center);
            Line(layout, 60, 110, 734, 110, 2);
            Text(layout, "LEDGER ACCOUNT", 60, 130, 674, 30, 20, bold: true, align: TemplateTextAlignment.Center);

            Text(layout, "Account         : {AccountName}", 100, 190, 300, 22, 12, bold: true);
            Text(layout, "Opening Balance : {OpeningBalance}", 100, 214, 300, 22, 12);
            Text(layout, "Closing Balance : {ClosingBalance}", 100, 238, 300, 22, 12, bold: true);
            Text(layout, "Period          : {FromDate} - {ToDate}", 100, 262, 300, 22, 12);

            Line(layout, 60, 300, 734, 300, 1);
            Text(layout, "Printed on  : {CreatedDate}", 80, 640, 614, 20, 11);
        }

        private static void BuildCover(PrintTemplateLayout layout)
        {
            Rectangle(layout, 60, 60, 674, 8, fill: "333333", border: "Transparent", borderThickness: 0);
            Text(layout, "{ReportTitle}", 60, 120, 674, 48, 28, bold: true, align: TemplateTextAlignment.Center);
            Line(layout, 247, 176, 547, 176, 1.5);

            Text(layout, "{CompanyName}", 60, 260, 674, 26, 16, bold: true, align: TemplateTextAlignment.Center);
            Text(layout, "GSTIN: {Gstin}", 60, 290, 674, 18, 11, align: TemplateTextAlignment.Center);
            Text(layout, "{Address}", 60, 310, 674, 18, 11, align: TemplateTextAlignment.Center);
            Text(layout, "{City}, {State} - {PinCode}", 60, 330, 674, 18, 11, align: TemplateTextAlignment.Center);
            Text(layout, "{Phone}   |   {Email}", 60, 350, 674, 18, 11, align: TemplateTextAlignment.Center);

            Line(layout, 60, 420, 734, 420, 1);
            Text(layout, "Period       : {FromDate} - {ToDate}", 120, 470, 300, 22, 12);
            Text(layout, "Version      : {Version}", 120, 494, 300, 22, 12);
            Text(layout, "Printed By   : {PrintedBy}", 120, 518, 300, 22, 12);
            Text(layout, "Printed on   : {CreatedDate}", 120, 542, 300, 22, 12);

            Rectangle(layout, 60, 950, 674, 4, fill: "333333", border: "Transparent", borderThickness: 0);
        }

        private static void BuildReport(PrintTemplateLayout layout)
        {
            Text(layout, "{CompanyName}", 60, 40, 674, 34, 22, bold: true, align: TemplateTextAlignment.Center);
            Text(layout, "GSTIN: {Gstin}", 60, 78, 674, 18, 11, align: TemplateTextAlignment.Center);
            Line(layout, 60, 110, 734, 110, 2);
            Text(layout, "{ReportTitle}", 60, 140, 674, 34, 20, bold: true, align: TemplateTextAlignment.Center);
            Text(layout, "Period : {FromDate} - {ToDate}", 60, 178, 674, 18, 11, align: TemplateTextAlignment.Center);

            Line(layout, 60, 600, 734, 600, 1);
            Text(layout, "Printed on  : {CreatedDate}", 80, 640, 614, 20, 11);
        }

        private static void Text(PrintTemplateLayout layout, string text, double x, double y, double width, double height,
            double fontSize, bool bold = false, string? color = null, TemplateTextAlignment align = TemplateTextAlignment.Left,
            double rotation = 0, double opacity = 1)
        {
            layout.Items.Add(new PrintTemplateItem
            {
                Kind = TemplateItemKind.Text,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Text = text,
                FontSize = fontSize,
                Bold = bold,
                TextColor = color,
                TextAlignment = align,
                Rotation = rotation,
                Opacity = opacity
            });
        }

        private static void Line(PrintTemplateLayout layout, double x1, double y1, double x2, double y2, double thickness,
            string? color = "333333")
        {
            layout.Items.Add(new PrintTemplateItem
            {
                Kind = TemplateItemKind.Line,
                X = x1,
                Y = y1,
                Width = x2 - x1,
                Height = y2 - y1,
                BorderColor = color,
                BorderThickness = thickness
            });
        }

        private static void Rectangle(PrintTemplateLayout layout, double x, double y, double width, double height,
            string? fill, string? border, double borderThickness)
        {
            layout.Items.Add(new PrintTemplateItem
            {
                Kind = TemplateItemKind.Rectangle,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                FillColor = fill,
                BorderColor = border,
                BorderThickness = borderThickness
            });
        }
    }
}
