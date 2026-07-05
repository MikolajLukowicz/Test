using System.Collections.Generic;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Models;

namespace SystemAukcyjny.Wpf.Services
{
    public interface IAuctionService
    {
        Task<IEnumerable<Aukcja>> GetAllAuctionsAsync();
        Task<IEnumerable<Aukcja>> GetAuctionsByCategoryAsync(int categoryId);
        Task<IEnumerable<Kategoria>> GetAllCategoriesAsync();
        Task<Aukcja?> GetAuctionByIdAsync(int auctionId);
        Task<bool> AddAuctionAsync(Aukcja auction);
        Task<bool> PlaceBidAsync(int auctionId, int userId, decimal amount);
        Task<bool> DeleteAuctionAsync(int auctionId);
        Task<bool> EndAuctionAsync(int auctionId);
        Task<IEnumerable<Aukcja>> GetUserAuctionsAsync(int userId);
        Task<IEnumerable<Aukcja>> GetUserBiddedAuctionsAsync(int userId);
        Task<IEnumerable<Aukcja>> GetUserWonAuctionsAsync(int userId);
    }
}
