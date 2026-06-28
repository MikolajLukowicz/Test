using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Data;
using SystemAukcyjny.Wpf.Models;

namespace SystemAukcyjny.Wpf.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly IDbContextFactory<AuctionDbContext> _contextFactory;

        public AuctionService(IDbContextFactory<AuctionDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Aukcja>> GetAllAuctionsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Aukcje
                .Include(a => a.Kategoria)
                .Include(a => a.Wystawca)
                .Include(a => a.Licytacje)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aukcja>> GetAuctionsByCategoryAsync(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Aukcje
                .Where(a => a.IdKategorii == categoryId)
                .Include(a => a.Kategoria)
                .Include(a => a.Wystawca)
                .Include(a => a.Licytacje)
                .ToListAsync();
        }

        public async Task<IEnumerable<Kategoria>> GetAllCategoriesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Kategorie.ToListAsync();
        }

        public async Task<Aukcja?> GetAuctionByIdAsync(int auctionId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Aukcje
                .Include(a => a.Kategoria)
                .Include(a => a.Wystawca)
                .Include(a => a.Licytacje)
                .ThenInclude(l => l.Licytujacy)
                .FirstOrDefaultAsync(a => a.IdAukcji == auctionId);
        }

        public async Task<bool> AddAuctionAsync(Aukcja auction)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Aukcje.Add(auction);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> PlaceBidAsync(int auctionId, int userId, decimal amount)
        {
            using var context = _contextFactory.CreateDbContext();
            var auction = await context.Aukcje
                .Include(a => a.Licytacje)
                .FirstOrDefaultAsync(a => a.IdAukcji == auctionId);

            if (auction == null || auction.Status != "Aktywna") return false;

            var highestBid = auction.Licytacje.OrderByDescending(l => l.KwotaLicytacji).FirstOrDefault();
            if (amount <= auction.CenaWywolawcza || (highestBid != null && amount <= highestBid.KwotaLicytacji))
            {
                return false;
            }

            var bid = new Licytacja
            {
                IdAukcji = auctionId,
                IdUzytkownika = userId,
                KwotaLicytacji = amount,
                DataZlozenia = DateTime.Now
            };

            context.Licytacje.Add(bid);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAuctionAsync(int auctionId)
        {
            using var context = _contextFactory.CreateDbContext();
            var auction = await context.Aukcje.FindAsync(auctionId);
            if (auction == null) return false;

            context.Aukcje.Remove(auction);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EndAuctionAsync(int auctionId)
        {
            using var context = _contextFactory.CreateDbContext();
            var auction = await context.Aukcje.FindAsync(auctionId);
            if (auction == null) return false;

            auction.Status = "Zakończona";
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Aukcja>> GetUserAuctionsAsync(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Aukcje
                .Where(a => a.IdUzytkownika == userId)
                .Include(a => a.Kategoria)
                .Include(a => a.Licytacje)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aukcja>> GetUserBiddedAuctionsAsync(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Aukcje
                .Where(a => a.Licytacje.Any(l => l.IdUzytkownika == userId))
                .Include(a => a.Kategoria)
                .Include(a => a.Licytacje)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aukcja>> GetUserWonAuctionsAsync(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            // A user won if the auction is ended and they have the highest bid
            var endedAuctions = await context.Aukcje
                .Where(a => a.Status == "Zakończona")
                .Include(a => a.Licytacje)
                .ToListAsync();

            return endedAuctions
                .Where(a => a.Licytacje.OrderByDescending(l => l.KwotaLicytacji).FirstOrDefault()?.IdUzytkownika == userId)
                .ToList();
        }
    }
}
