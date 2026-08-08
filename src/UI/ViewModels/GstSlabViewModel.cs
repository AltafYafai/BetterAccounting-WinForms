using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class GstSlabViewModel : ViewModelBase
    {
        private readonly SQLiteContext _context;
        private readonly GstSlabRepository _repository;

        private ObservableCollection<GstSlab> _slabs = new();
        private GstSlab? _selectedSlab;
        private string _slabName = string.Empty;
        private decimal _slabRate;
        private string _statusMessage = string.Empty;

        public GstSlabViewModel()
        {
            _context = new SQLiteContext(GetDatabasePath());
            _repository = new GstSlabRepository(_context.Connection);
            AddCommand = new RelayCommand(async () => await AddAsync());
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => SelectedSlab is not null);
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
                await _repository.SeedDefaultsAsync();
                var list = await _repository.GetAsync();
                Slabs = new ObservableCollection<GstSlab>(list);
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Load GST slabs", ex);
            }
        }

        private async Task AddAsync()
        {
            if (string.IsNullOrWhiteSpace(SlabName))
                return;

            try
            {
                var slab = new GstSlab { Name = SlabName, Rate = SlabRate };
                await _repository.AddAsync(slab);
                Slabs.Add(slab);
                SlabName = string.Empty;
                SlabRate = 0m;
                StatusMessage = $"GST slab '{slab.Name}' added.";
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message($"Add GST slab '{SlabName}'", ex);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedSlab is null)
                return;

            try
            {
                await _repository.DeleteAsync(SelectedSlab.Id);
                var name = SelectedSlab.Name;
                Slabs.Remove(SelectedSlab);
                SelectedSlab = null;
                StatusMessage = $"GST slab '{name}' deleted.";
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message($"Delete GST slab '{SelectedSlab?.Name}'", ex);
            }
        }

        public ObservableCollection<GstSlab> Slabs { get => _slabs; set => SetProperty(ref _slabs, value); }

        public GstSlab? SelectedSlab
        {
            get => _selectedSlab;
            set
            {
                if (SetProperty(ref _selectedSlab, value))
                    DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public string SlabName { get => _slabName; set => SetProperty(ref _slabName, value); }
        public decimal SlabRate { get => _slabRate; set => SetProperty(ref _slabRate, value); }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
    }
}