using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Models;
using SystemAukcyjny.Wpf.Services;

namespace SystemAukcyjny.Wpf.ViewModels
{
    public partial class AuctionItemViewModel : ViewModelBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IAuthService _authService;
        private readonly AuctionListViewModel _parent;

        [ObservableProperty]
        private Aukcja _auction;

        [ObservableProperty]
        private string _bidAmount = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private decimal _currentHighestBid;

        public AuctionItemViewModel(Aukcja auction, IAuctionService auctionService, IAuthService authService, AuctionListViewModel parent)
        {
            _auction = auction;
            _auctionService = auctionService;
            _authService = authService;
            _parent = parent;
            UpdateHighestBid();
        }

        private void UpdateHighestBid()
        {
            var maxBid = Auction.Licytacje.Any()
                ? Auction.Licytacje.Max(l => l.KwotaLicytacji)
                : Auction.CenaWywolawcza;
            CurrentHighestBid = maxBid;
        }

        [RelayCommand]
        private async Task PlaceBid()
        {
            // Use CultureInfo.CurrentCulture to handle comma vs dot separators based on system settings
            if (decimal.TryParse(BidAmount, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal amount) ||
                decimal.TryParse(BidAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
            {
                if (amount <= CurrentHighestBid)
                {
                    ErrorMessage = $"Kwota musi być większa niż {CurrentHighestBid:C}";
                    return;
                }

                try
                {
                    var success = await _auctionService.PlaceBidAsync(Auction.IdAukcji, _authService.CurrentUser!.IdUzytkownika, amount);
                    if (success)
                    {
                        await _parent.FilterAuctions();
                        ErrorMessage = string.Empty;
                        BidAmount = string.Empty;
                    }
                    else
                    {
                        ErrorMessage = "Kwota za niska lub aukcja zakończona.";
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Błąd licytacji: {ex.InnerException?.Message ?? ex.Message}";
                }
            }
            else
            {
                ErrorMessage = "Nieprawidłowa kwota.";
            }
        }
    }

    public partial class AuctionListViewModel : ViewModelBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<AuctionItemViewModel> _auctions = new();

        [ObservableProperty]
        private ObservableCollection<Kategoria> _categories = new();

        [ObservableProperty]
        private Kategoria? _selectedCategory;

        public AuctionListViewModel(IAuctionService auctionService, IAuthService authService)
        {
            _auctionService = auctionService;
            _authService = authService;
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadData()
        {
            try
            {
                var categories = await _auctionService.GetAllCategoriesAsync();
                Categories = new ObservableCollection<Kategoria>(categories);
                await FilterAuctions();
            }
            catch (Exception)
            {
            }
        }

        [RelayCommand]
        public async Task FilterAuctions()
        {
            try
            {
                IEnumerable<Aukcja> auctions;
                if (SelectedCategory != null)
                {
                    auctions = await _auctionService.GetAuctionsByCategoryAsync(SelectedCategory.IdKategorii);
                }
                else
                {
                    auctions = await _auctionService.GetAllAuctionsAsync();
                }
                Auctions = new ObservableCollection<AuctionItemViewModel>(
                    auctions.Select(a => new AuctionItemViewModel(a, _auctionService, _authService, this))
                );
            }
            catch (Exception)
            {
            }
        }
    }
}
