using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Data;
using SystemAukcyjny.Wpf.Models;

namespace SystemAukcyjny.Wpf.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbContextFactory<AuctionDbContext> _contextFactory;
        public Uzytkownik? CurrentUser { get; private set; }

        public AuthService(IDbContextFactory<AuctionDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Uzytkownik?> LoginAsync(string login, string password)
        {
            using var context = _contextFactory.CreateDbContext();
            // In a real application, passwords should be hashed!
            var user = await context.Uzytkownicy.FirstOrDefaultAsync(u => u.Login == login && u.Haslo == password);
            CurrentUser = user;
            return user;
        }

        public async Task<bool> RegisterAsync(string login, string password, string email)
        {
            using var context = _contextFactory.CreateDbContext();
            if (await context.Uzytkownicy.AnyAsync(u => u.Login == login || u.Email == email))
            {
                return false;
            }

            var newUser = new Uzytkownik
            {
                Login = login,
                Haslo = password, // In a real application, passwords should be hashed!
                Email = email
            };

            context.Uzytkownicy.Add(newUser);
            await context.SaveChangesAsync();
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
