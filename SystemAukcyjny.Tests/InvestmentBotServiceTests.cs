using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SystemAukcyjny.Wpf.Models;
using SystemAukcyjny.Wpf.Services;
using Xunit;

namespace SystemAukcyjny.Tests
{
    public class InvestmentBotServiceTests
    {
        private readonly Mock<IAuctionService> _mockAuctionService;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly InvestmentBotService _botService;

        public InvestmentBotServiceTests()
        {
            _mockAuctionService = new Mock<IAuctionService>();
            _mockAuthService = new Mock<IAuthService>();
            _botService = new InvestmentBotService(_mockAuctionService.Object, _mockAuthService.Object);
        }

        [Fact]
        public void AnalyzeAuction_CalculatesFairValueAndRoiCorrectly()
        {
            // Arrange
            var auction = new Aukcja
            {
                IdAukcji = 1,
                Tytul = "Laptop Gamingowy i7 RTX",
                OpisAukcji = "Bardzo mocny laptop w świetnym stanie, bardzo polecam ten model do gier i pracy.",
                CenaWywolawcza = 1000m,
                DataRozpoczecia = DateTime.Now.AddDays(-2),
                DataZakonczenia = DateTime.Now.AddDays(2),
                IdKategorii = 1,
                IdUzytkownika = 10,
                Status = "Aktywna",
                Licytacje = new List<Licytacja>
                {
                    new Licytacja { KwotaLicytacji = 1200m, DataZlozenia = DateTime.Now.AddHours(-1) }
                }
            };

            var categoryAuctions = new List<Aukcja> { auction };

            // Act
            var analysis = _botService.AnalyzeAuction(auction, categoryAuctions);

            // Assert
            Assert.NotNull(analysis);
            Assert.Equal(1200m, analysis.CurrentPrice);
            Assert.Equal(1210m, analysis.NextBidAmount); // 1200 + step 10 = 1210
            Assert.True(analysis.EstimatedFairValue > 1200m);
            Assert.True(analysis.PriceToValueRatio < 1.0);
            Assert.True(analysis.DemandMomentumIndex >= 0 && analysis.DemandMomentumIndex <= 100);
            Assert.True(analysis.RiskScore >= 0 && analysis.RiskScore <= 100);
        }

        [Fact]
        public void AnalyzeAuction_GeneratesBuySignalWhenRoiAndPvRatioAreFavorable()
        {
            // Arrange
            var auction = new Aukcja
            {
                IdAukcji = 2,
                Tytul = "Smartfon Flagowiec",
                OpisAukcji = "Nowy smartfon z gwarancją producenta",
                CenaWywolawcza = 200m,
                DataRozpoczecia = DateTime.Now.AddDays(-1),
                DataZakonczenia = DateTime.Now.AddDays(5),
                IdKategorii = 2,
                IdUzytkownika = 20,
                Status = "Aktywna",
                Licytacje = new List<Licytacja>()
            };

            var categoryAuctions = new List<Aukcja>
            {
                auction,
                new Aukcja { CenaWywolawcza = 800m, IdKategorii = 2 }
            };

            _botService.Settings.Strategy = "Agresywna";
            _botService.Settings.TargetMinRoiPercentage = 10m;

            // Act
            var analysis = _botService.AnalyzeAuction(auction, categoryAuctions);

            // Assert
            Assert.StartsWith("KUP", analysis.Signal);
            Assert.True(analysis.ExpectedRoiPercentage >= 10m);
        }

        [Fact]
        public async Task RunSingleAnalysisCycleAsync_ExecutesBidsWhenOpportunityFound()
        {
            // Arrange
            var currentUser = new Uzytkownik { IdUzytkownika = 99, Login = "InvestorUser" };
            _mockAuthService.Setup(a => a.CurrentUser).Returns(currentUser);

            var auction = new Aukcja
            {
                IdAukcji = 5,
                Tytul = "Karta Graficzna RTX",
                OpisAukcji = "Używana karta w idealnym stanie z oryginalnym pudełkiem i gwarancją producenta.",
                CenaWywolawcza = 100m,
                DataRozpoczecia = DateTime.Now.AddHours(-5),
                DataZakonczenia = DateTime.Now.AddHours(24),
                IdKategorii = 3,
                IdUzytkownika = 50,
                Status = "Aktywna",
                Licytacje = new List<Licytacja>()
            };

            var categoryAuctions = new List<Aukcja>
            {
                auction,
                new Aukcja { CenaWywolawcza = 1000m, IdKategorii = 3 }
            };

            _mockAuctionService.Setup(s => s.GetAllAuctionsAsync()).ReturnsAsync(categoryAuctions);
            _mockAuctionService.Setup(s => s.PlaceBidAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()))
                .ReturnsAsync(true);

            _botService.Settings.AutoBidEnabled = true;
            _botService.Settings.MaxBidLimit = 2000m;
            _botService.Settings.TargetMinRoiPercentage = 10m;

            // Act
            var analyses = await _botService.RunSingleAnalysisCycleAsync();

            // Assert
            Assert.NotEmpty(analyses);
            _mockAuctionService.Verify(s => s.PlaceBidAsync(5, 99, It.IsAny<decimal>()), Times.Once);
        }

        [Fact]
        public async Task StartAndStopAsync_TogglesBotState()
        {
            // Act
            Assert.False(_botService.IsRunning);

            await _botService.StartAsync();
            Assert.True(_botService.IsRunning);

            await _botService.StopAsync();
            Assert.False(_botService.IsRunning);
        }
    }
}
