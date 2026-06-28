using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Models;
using SystemAukcyjny.Wpf.Services;

namespace SystemAukcyjny.Wpf.ViewModels
{
    public partial class MyAuctionsViewModel : ViewModelBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<Aukcja> _myAuctions = new();

        [ObservableProperty]
        private ObservableCollection<Aukcja> _biddedAuctions = new();

        [ObservableProperty]
        private ObservableCollection<Aukcja> _wonAuctions = new();

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
            MyAuctions = new ObservableCollection<Aukcja>(await _auctionService.GetUserAuctionsAsync(userId));
            BiddedAuctions = new ObservableCollection<Aukcja>(await _auctionService.GetUserBiddedAuctionsAsync(userId));
            WonAuctions = new ObservableCollection<Aukcja>(await _auctionService.GetUserWonAuctionsAsync(userId));
        }

        [RelayCommand]
        private async Task DeleteAuction(Aukcja auction)
        {
            if (await _auctionService.DeleteAuctionAsync(auction.IdAukcji))
            {
                await LoadData();
            }
        }

        [RelayCommand]
        private async Task EndAuction(Aukcja auction)
        {
            if (await _auctionService.EndAuctionAsync(auction.IdAukcji))
            {
                await LoadData();
            }
        }
    }
}
