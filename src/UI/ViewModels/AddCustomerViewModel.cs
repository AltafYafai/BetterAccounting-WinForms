using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class AddCustomerViewModel : ViewModelBase
    {
        private readonly SQLiteContext _context;
        private readonly CustomerRepository _repository;
        private readonly GstinLookupService _lookupService;

        private string _name = string.Empty;
        private string _gstin = string.Empty;
        private string _address = string.Empty;
        private string _city = string.Empty;
        private string _state = string.Empty;
        private string _pinCode = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private bool _isBusy;
        private string _statusMessage = string.Empty;

        public AddCustomerViewModel()
        {
            _context = new SQLiteContext(GetDatabasePath());
            _repository = new CustomerRepository(_context.Connection);
            _lookupService = new GstinLookupService(new HttpClient());

            FetchCommand = new RelayCommand(async () => await FetchFromGstinAsync(), () => !IsBusy);
            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !IsBusy);
        }

        public AddCustomerViewModel(CustomerRepository repository, GstinLookupService lookupService)
        {
            _repository = repository;
            _lookupService = lookupService;

            FetchCommand = new RelayCommand(async () => await FetchFromGstinAsync(), () => !IsBusy);
            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !IsBusy);
        }

        private static string GetDatabasePath()
        {
            return BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();
        }

        private async Task FetchFromGstinAsync()
        {
            IsBusy = true;
            StatusMessage = "Looking up GSTIN...";
            try
            {
                var result = await _lookupService.LookupAsync(Gstin);
                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Name = !string.IsNullOrWhiteSpace(result.LegalName) ? result.LegalName : result.TradeName;
                    Address = result.Address;
                    City = result.City;
                    State = result.State;
                    PinCode = result.PinCode;
                    StatusMessage = "Company data loaded. Review and save.";
                }
                else
                {
                    StatusMessage = result.ErrorMessage;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAsync()
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            try
            {
                var customer = new Customer
                {
                    Name = Name,
                    Gstin = Gstin,
                    Address = Address,
                    City = City,
                    State = State,
                    PinCode = PinCode,
                    Phone = Phone,
                    Email = Email
                };
                await _repository.AddAsync(customer);
                StatusMessage = "Customer saved.";
                ClearForm();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Gstin = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            State = string.Empty;
            PinCode = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
        }

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Gstin { get => _gstin; set => SetProperty(ref _gstin, value); }
        public string Address { get => _address; set => SetProperty(ref _address, value); }
        public string City { get => _city; set => SetProperty(ref _city, value); }
        public string State { get => _state; set => SetProperty(ref _state, value); }
        public string PinCode { get => _pinCode; set => SetProperty(ref _pinCode, value); }
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    FetchCommand.RaiseCanExecuteChanged();
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand FetchCommand { get; }
        public RelayCommand SaveCommand { get; }
    }
}