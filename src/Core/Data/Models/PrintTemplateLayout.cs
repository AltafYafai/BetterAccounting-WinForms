using System.Collections.ObjectModel;

namespace BetterAccounting.Core.Data.Models
{
    public class PrintTemplateLayout
    {
        public const double DefaultPageWidth = 794;
        public const double DefaultPageHeight = 1123;

        public double PageWidth { get; set; } = DefaultPageWidth;
        public double PageHeight { get; set; } = DefaultPageHeight;
        public string PageName { get; set; } = "A4";
        public bool Landscape { get; set; }
        public ObservableCollection<PrintTemplateItem> Items { get; set; } = new();

        public static PrintTemplateLayout CreateDefault()
        {
            return new PrintTemplateLayout
            {
                PageWidth = DefaultPageWidth,
                PageHeight = DefaultPageHeight,
                PageName = "A4",
                Landscape = false
            };
        }

        public static (double Width, double Height, string Name) GetPageSize(string pageName, bool landscape)
        {
            var (width, height) = pageName switch
            {
                "A5" => (559.0, 794.0),
                "A4" => (794.0, 1123.0),
                "A3" => (1123.0, 1587.0),
                "Letter" => (816.0, 1056.0),
                "Legal" => (816.0, 1344.0),
                _ => (794.0, 1123.0)
            };
            if (landscape)
                (width, height) = (height, width);
            return (width, height, pageName);
        }
    }
}
