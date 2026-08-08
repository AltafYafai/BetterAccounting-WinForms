using System.Collections.Generic;
using Avalonia.Controls;

namespace BetterAccounting.UI.Models
{
    public class PrintDocumentModel
    {
        public List<Control> Pages { get; } = new();
    }
}
