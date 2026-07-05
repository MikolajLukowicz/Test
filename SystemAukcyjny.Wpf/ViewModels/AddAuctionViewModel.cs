using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Models;
using SystemAukcyjny.Wpf.Services;

namespace SystemAukcyjny.Wpf.ViewModels
{
    public partial class AddAuctionViewModel : ViewModelBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private decimal _startingPrice = 0.01m;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now.AddDays(7);

        [ObservableProperty]
        private ObservableCollection<Kategoria> _categories = new();

        [ObservableProperty]
        private Kategoria? _selectedCategory;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public AddAuctionViewModel(IAuctionService auctionService, IAuthService authService, INavigationService navigationService)
        {
            _auctionService = auctionService;
            _authService = authService;
            _navigationService = navigationService;
            LoadCategoriesCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadCategories()
        {
            try
            {
                var categories = await _auctionService.GetAllCategoriesAsync();
                Categories = new ObservableCollection<Kategoria>(categories);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd ładowania kategorii: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task AddAuction()
        {
            if (SelectedCategory == null)
            {
                ErrorMessage = "Wybierz kategorię.";
                return;
            }
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Tytuł jest wymagany.";
                return;
            }
            if (StartingPrice < 0.01m)
            {
                ErrorMessage = "Cena wywoławcza musi wynosić co najmniej 0.01.";
                return;
            }
            if (EndDate <= DateTime.Now)
            {
                ErrorMessage = "Data zakończenia musi być w przyszłości.";
                return;
            }

            try
            {
                var auction = new Aukcja
                {
                    Tytul = Title,
                    OpisAukcji = Description,
                    CenaWywolawcza = StartingPrice,
                    DataRozpoczecia = DateTime.Now,
                    DataZakonczenia = EndDate,
                    IdUzytkownika = _authService.CurrentUser!.IdUzytkownika,
                    IdKategorii = SelectedCategory.IdKategorii,
                    Status = "Aktywna"
                };

                if (await _auctionService.AddAuctionAsync(auction))
                {
                    _navigationService.NavigateTo<AuctionListViewModel>();
                }
                else
                {
                    ErrorMessage = "Nie udało się wystawić aukcji.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas wystawiania: {ex.Message}";
            }
        }
    }
}
