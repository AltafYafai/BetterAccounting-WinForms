using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace BetterAccounting.UI.Services
{
    public static class FileDialogService
    {
        public static async Task<string?> PickFileAsync(string title, params (string Name, string[] Extensions)[] filters)
        {
            var provider = AppServices.StorageProvider;
            if (provider == null) return null;

            var result = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = BuildFilters(filters)
            });

            return result.FirstOrDefault()?.TryGetLocalPath();
        }

        public static async Task<string?> PickFolderAsync(string title)
        {
            var provider = AppServices.StorageProvider;
            if (provider == null) return null;

            var result = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            return result.FirstOrDefault()?.TryGetLocalPath();
        }

        public static async Task<string?> SaveFileAsync(string title, string? suggestedName, params (string Name, string[] Extensions)[] filters)
        {
            var provider = AppServices.StorageProvider;
            if (provider == null) return null;

            var result = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedName,
                FileTypeChoices = BuildFilters(filters)
            });

            return result?.TryGetLocalPath();
        }

        private static List<FilePickerFileType> BuildFilters((string Name, string[] Extensions)[] filters)
        {
            if (filters == null || filters.Length == 0)
                return new List<FilePickerFileType>();

            return filters
                .Select(f => new FilePickerFileType(f.Name) { Patterns = f.Extensions })
                .ToList();
        }
    }
}
