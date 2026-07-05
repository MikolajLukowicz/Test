using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Models;
using SystemAukcyjny.Wpf.Services;

namespace SystemAukcyjny.Wpf.ViewModels
{
    public partial class MyAuctionItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Aukcja _auction;

        [ObservableProperty]
        private decimal _winningAmount;

        public MyAuctionItemViewModel(Aukcja auction)
        {
            _auction = auction;
            // Get the highest bid or starting price
            _winningAmount = auction.Licytacje.Any()
                ? auction.Licytacje.Max(l => l.KwotaLicytacji)
                : auction.CenaWywolawcza;
        }
    }

    public partial class MyAuctionsViewModel : ViewModelBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<MyAuctionItemViewModel> _myAuctions = new();

        [ObservableProperty]
        private ObservableCollection<Aukcja> _biddedAuctions = new();

        [ObservableProperty]
        private ObservableCollection<MyAuctionItemViewModel> _wonAuctions = new();

        public MyAuctionsViewModel(IAuctionService auctionService, IAuthService authService)
        {
            _auctionService = auctionService;
            _authService = authService;
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadData()
        {
            int userId = _authService.CurrentUser!.IdUzytkownika;

            var owned = await _auctionService.GetUserAuctionsAsync(userId);
            MyAuctions = new ObservableCollection<MyAuctionItemViewModel>(owned.Select(a => new MyAuctionItemViewModel(a)));

            BiddedAuctions = new ObservableCollection<Aukcja>(await _auctionService.GetUserBiddedAuctionsAsync(userId));

            var won = await _auctionService.GetUserWonAuctionsAsync(userId);
            WonAuctions = new ObservableCollection<MyAuctionItemViewModel>(won.Select(a => new MyAuctionItemViewModel(a)));
        }

        [RelayCommand]
        private async Task DeleteAuction(MyAuctionItemViewModel item)
        {
            if (await _auctionService.DeleteAuctionAsync(item.Auction.IdAukcji))
            {
                await LoadData();
            }
        }

        [RelayCommand]
        private async Task EndAuction(MyAuctionItemViewModel item)
        {
            if (await _auctionService.EndAuctionAsync(item.Auction.IdAukcji))
            {
                await LoadData();
            }
        }
    }
}
