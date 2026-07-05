using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SystemAukcyjny.Wpf.Services;

namespace SystemAukcyjny.Wpf.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ViewModelBase? _currentContentViewModel;

        [ObservableProperty]
        private bool _isUserLoggedIn;

        public MainViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            _navigationService.CurrentViewModelChanged += () => {
                CurrentContentViewModel = _navigationService.CurrentViewModel;
                IsUserLoggedIn = _authService.CurrentUser != null;
            };

            // Explicitly set initial state
            IsUserLoggedIn = _authService.CurrentUser != null;

            // Start with login
            _navigationService.NavigateTo<LoginViewModel>();
        }

        [RelayCommand]
        private void GoToAuctions() => _navigationService.NavigateTo<AuctionListViewModel>();

        [RelayCommand]
        private void GoToMyAuctions() => _navigationService.NavigateTo<MyAuctionsViewModel>();

        [RelayCommand]
        private void GoToAddAuction() => _navigationService.NavigateTo<AddAuctionViewModel>();

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            _navigationService.NavigateTo<LoginViewModel>();
        }
    }
}
