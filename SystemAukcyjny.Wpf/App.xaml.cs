using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemAukcyjny.Wpf.Data;
using SystemAukcyjny.Wpf.Services;
using SystemAukcyjny.Wpf.ViewModels;

namespace SystemAukcyjny.Wpf
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            // Configure Database
            string connectionString = "Server=DESKTOP-S98QKHV;Database=SystemAukcyjny;Trusted_Connection=True;TrustServerCertificate=True;";
            services.AddDbContextFactory<AuctionDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Configure Services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAuctionService, AuctionService>();

            // Configure ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<AuctionListViewModel>();
            services.AddTransient<MyAuctionsViewModel>();
            services.AddTransient<AddAuctionViewModel>();

            // Configure Main Window
            services.AddSingleton<MainWindow>(s => new MainWindow()
            {
                DataContext = s.GetRequiredService<MainViewModel>()
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
