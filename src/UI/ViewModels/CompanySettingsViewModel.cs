using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class CompanySettingsViewModel : ViewModelBase
    {
        private readonly SQLiteContext _context;
        private readonly CompanyProfileRepository _repository;

        private string _companyName = string.Empty;
        private string _gstin = string.Empty;
        private string _address = string.Empty;
        private string _city = string.Empty;
        private string _state = string.Empty;
        private string _pinCode = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _contactPerson = string.Empty;
        private string _statusMessage = string.Empty;

        public CompanySettingsViewModel()
        {
            var dbPath = GetDatabasePath();
            _context = new SQLiteContext(dbPath);
            _repository = new CompanyProfileRepository(_context.Connection);
            SaveCommand = new RelayCommand(async () => await SaveAsync());
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
                var profile = await _repository.GetAsync();
                if (profile is null)
                    return;

                CompanyName = profile.CompanyName;
                Gstin = profile.Gstin;
                Address = profile.Address;
                City = profile.City;
                State = profile.State;
                PinCode = profile.PinCode;
                Phone = profile.Phone;
                Email = profile.Email;
                ContactPerson = profile.ContactPerson;
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Load company profile", ex);
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                var profile = new CompanyProfile
                {
                    CompanyName = CompanyName,
                    Gstin = Gstin,
                    Address = Address,
                    City = City,
                    State = State,
                    PinCode = PinCode,
                    Phone = Phone,
                    Email = Email,
                    ContactPerson = ContactPerson
                };
                await _repository.SaveAsync(profile);
                StatusMessage = "Company profile saved.";
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Save company profile", ex);
            }
        }

        public string CompanyName { get => _companyName; set => SetProperty(ref _companyName, value); }
        public string Gstin { get => _gstin; set => SetProperty(ref _gstin, value); }
        public string Address { get => _address; set => SetProperty(ref _address, value); }
        public string City { get => _city; set => SetProperty(ref _city, value); }
        public string State { get => _state; set => SetProperty(ref _state, value); }
        public string PinCode { get => _pinCode; set => SetProperty(ref _pinCode, value); }
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string ContactPerson { get => _contactPerson; set => SetProperty(ref _contactPerson, value); }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SaveCommand { get; }
    }
}