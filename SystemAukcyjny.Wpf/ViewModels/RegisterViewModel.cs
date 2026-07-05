using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Services;

namespace SystemAukcyjny.Wpf.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _login = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public RegisterViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Wszystkie pola są wymagane.";
                return;
            }

            try
            {
                var success = await _authService.RegisterAsync(Login, Password, Email);
                if (success)
                {
                    _navigationService.NavigateTo<LoginViewModel>();
                }
                else
                {
                    ErrorMessage = "Użytkownik o podanym loginie lub emailu już istnieje.";
                }
            }
            catch (Exception ex)
            {
                // Catch database constraints (like email format CHECK constraint)
                ErrorMessage = $"Błąd rejestracji: {ex.InnerException?.Message ?? ex.Message}";
            }
        }

        [RelayCommand]
        private void GoToLogin()
        {
            _navigationService.NavigateTo<LoginViewModel>();
        }
    }
}
