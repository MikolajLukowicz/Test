using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Models;

namespace SystemAukcyjny.Wpf.Services
{
    public interface IAuthService
    {
        Task<Uzytkownik?> LoginAsync(string login, string password);
        Task<bool> RegisterAsync(string login, string password, string email);
        Uzytkownik? CurrentUser { get; }
        void Logout();
    }
}
