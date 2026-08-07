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
            await _repository.SeedDefaultsAsync();
            var list = await _repository.GetAsync();
            Slabs = new ObservableCollection<GstSlab>(list);
        }

        private async Task AddAsync()
        {
            if (string.IsNullOrWhiteSpace(SlabName))
                return;

            var slab = new GstSlab { Name = SlabName, Rate = SlabRate };
            await _repository.AddAsync(slab);
            Slabs.Add(slab);
            SlabName = string.Empty;
            SlabRate = 0m;
        }

        private async Task DeleteAsync()
        {
            if (SelectedSlab is null)
                return;

            await _repository.DeleteAsync(SelectedSlab.Id);
            Slabs.Remove(SelectedSlab);
            SelectedSlab = null;
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

        public ICommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
    }
}