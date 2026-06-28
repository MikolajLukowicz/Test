using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Services;

namespace SystemAukcyjny.Wpf.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _userLogin = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task Authenticate()
        {
            var user = await _authService.LoginAsync(UserLogin, Password);
            if (user != null)
            {
                // Navigate to AuctionList after successful login
                _navigationService.NavigateTo<AuctionListViewModel>();
            }
            else
            {
                ErrorMessage = "Nieprawidłowy login lub hasło.";
            }
        }

        [RelayCommand]
        private void GoToRegister()
        {
            _navigationService.NavigateTo<RegisterViewModel>();
        }
    }
}
