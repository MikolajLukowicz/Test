using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Models;

namespace SystemAukcyjny.Wpf.Services
{
    public class InvestmentBotService : IInvestmentBotService
    {
        private readonly IAuctionService _auctionService;
        private readonly IAuthService _authService;

        private readonly List<BotLogEntry> _logs = new();
        private readonly List<AuctionAnalysis> _currentAnalyses = new();
        private readonly object _lockObj = new();

        private CancellationTokenSource? _cts;
        private bool _isRunning;

        public InvestmentBotSettings Settings { get; } = new();

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnRunningStateChanged?.Invoke(_isRunning);
                }
            }
        }

        public IReadOnlyList<BotLogEntry> Logs
        {
            get
            {
                lock (_lockObj)
                {
                    return _logs.ToList();
                }
            }
        }

        public IReadOnlyList<AuctionAnalysis> CurrentAnalyses
        {
            get
            {
                lock (_lockObj)
                {
                    return _currentAnalyses.ToList();
                }
            }
        }

        public event Action<IEnumerable<AuctionAnalysis>>? OnAnalysisUpdated;
        public event Action<BotLogEntry>? OnLogAdded;
        public event Action<bool>? OnRunningStateChanged;

        public InvestmentBotService(IAuctionService auctionService, IAuthService authService)
        {
            _auctionService = auctionService;
            _authService = authService;
        }

        public Task StartAsync()
        {
            if (IsRunning) return Task.CompletedTask;

            IsRunning = true;
            _cts = new CancellationTokenSource();

            AddLog("INFO", "Bot inwestycyjny został uruchomiony.");

            _ = RunBotLoopAsync(_cts.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            if (!IsRunning) return Task.CompletedTask;

            _cts?.Cancel();
            IsRunning = false;
            AddLog("INFO", "Bot inwestycyjny został zatrzymany.");
            return Task.CompletedTask;
        }

        private async Task RunBotLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && IsRunning)
            {
                try
                {
                    await RunSingleAnalysisCycleAsync();
                }
                catch (Exception ex)
                {
                    AddLog("ERROR", $"Błąd podczas pętli bota: {ex.Message}");
                }

                int delaySeconds = Math.Max(1, Settings.ScanIntervalSeconds);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        public async Task<IEnumerable<AuctionAnalysis>> RunSingleAnalysisCycleAsync()
        {
            var auctions = (await _auctionService.GetAllAuctionsAsync())
                .Where(a => a.Status == "Aktywna" && a.DataZakonczenia > DateTime.Now)
                .ToList();

            var analyses = new List<AuctionAnalysis>();

            // Group auctions by category for relative price estimation
            var categoryGroup = auctions.GroupBy(a => a.IdKategorii).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var auction in auctions)
            {
                var catAuctions = categoryGroup.TryGetValue(auction.IdKategorii, out var list) ? list : new List<Aukcja>();
                var analysis = AnalyzeAuction(auction, catAuctions);
                analyses.Add(analysis);
            }

            lock (_lockObj)
            {
                _currentAnalyses.Clear();
                _currentAnalyses.AddRange(analyses);
            }

            OnAnalysisUpdated?.Invoke(analyses);

            // If auto-bid is enabled and bot is running, execute investment decisions
            if (Settings.AutoBidEnabled && _authService.CurrentUser != null)
            {
                await ExecuteInvestmentDecisionsAsync(analyses);
            }

            return analyses;
        }

        public AuctionAnalysis AnalyzeAuction(Aukcja auction, IEnumerable<Aukcja> categoryAuctions)
        {
            var bids = auction.Licytacje?.OrderByDescending(l => l.KwotaLicytacji).ToList() ?? new List<Licytacja>();
            decimal highestBid = bids.FirstOrDefault()?.KwotaLicytacji ?? 0m;
            decimal currentPrice = Math.Max(auction.CenaWywolawcza, highestBid);

            // Step bid calculation
            decimal step = currentPrice switch
            {
                < 50m => 1m,
                < 200m => 2m,
                < 500m => 5m,
                < 2000m => 10m,
                _ => 25m
            };

            decimal nextBid = highestBid > 0 ? highestBid + step : auction.CenaWywolawcza;

            // 1. Fair Market Value Indicator
            decimal estimatedFairValue = CalculateFairMarketValue(auction, categoryAuctions);

            // 2. Expected ROI Indicator (%)
            decimal expectedRoi = nextBid > 0 ? ((estimatedFairValue - nextBid) / nextBid) * 100m : 0m;

            // 3. Price-to-Value (P/V) Ratio
            double pvRatio = estimatedFairValue > 0 ? (double)(currentPrice / estimatedFairValue) : 1.0;

            // 4. Demand Momentum Index (0 - 100)
            double momentum = CalculateDemandMomentum(auction, bids);

            // 5. Risk Score (0 - 100)
            double risk = CalculateRiskScore(auction, bids, pvRatio, momentum);

            // 6. Signal Determination
            string signal = DetermineSignal(expectedRoi, pvRatio, risk, Settings.Strategy, Settings.TargetMinRoiPercentage);

            string summary = $"FairVal: {estimatedFairValue:C2} | ROI: {expectedRoi:F1}% | Momentum: {momentum:F0}/100 | Risk: {risk:F0}/100 | P/V: {pvRatio:F2}";

            return new AuctionAnalysis
            {
                Auction = auction,
                CurrentPrice = currentPrice,
                NextBidAmount = nextBid,
                EstimatedFairValue = estimatedFairValue,
                ExpectedRoiPercentage = Math.Round(expectedRoi, 2),
                DemandMomentumIndex = Math.Round(momentum, 1),
                PriceToValueRatio = Math.Round(pvRatio, 2),
                RiskScore = Math.Round(risk, 1),
                Signal = signal,
                AnalysisSummary = summary
            };
        }

        private decimal CalculateFairMarketValue(Aukcja auction, IEnumerable<Aukcja> categoryAuctions)
        {
            var catList = categoryAuctions?.ToList() ?? new List<Aukcja>();

            decimal baseCatAvg = 0m;
            if (catList.Count > 0)
            {
                var catPrices = catList.Select(a => Math.Max(a.CenaWywolawcza, a.Licytacje?.Max(l => (decimal?)l.KwotaLicytacji) ?? a.CenaWywolawcza));
                baseCatAvg = catPrices.Average();
            }

            // Estimator combines starting price multiplier and category baseline
            decimal valuationByStart = auction.CenaWywolawcza * 1.35m;
            decimal valuationByCat = baseCatAvg > 0 ? baseCatAvg * 1.15m : valuationByStart;

            decimal estimatedValue = Math.Max(valuationByStart, valuationByCat);

            // If auction description or title has premium indicators (e.g., long detailed description)
            if (!string.IsNullOrWhiteSpace(auction.OpisAukcji) && auction.OpisAukcji.Length > 50)
            {
                estimatedValue *= 1.05m;
            }

            return Math.Round(estimatedValue, 2);
        }

        private double CalculateDemandMomentum(Aukcja auction, List<Licytacja> bids)
        {
            int bidCount = bids.Count;
            TimeSpan timeElapsed = DateTime.Now - auction.DataRozpoczecia;
            double hoursElapsed = Math.Max(0.1, timeElapsed.TotalHours);
            double bidVelocity = bidCount / hoursElapsed; // bids per hour

            TimeSpan timeRemaining = auction.DataZakonczenia - DateTime.Now;
            double hoursRemaining = Math.Max(0, timeRemaining.TotalHours);

            // Velocity component (0-50)
            double velocityScore = Math.Min(50.0, bidVelocity * 25.0);

            // Urgency component (0-30) - higher score as deadline approaches
            double urgencyScore = hoursRemaining <= 24 ? (1.0 - (hoursRemaining / 24.0)) * 30.0 : 0.0;

            // Volume component (0-20)
            double volumeScore = Math.Min(20.0, bidCount * 4.0);

            return Math.Min(100.0, velocityScore + urgencyScore + volumeScore);
        }

        private double CalculateRiskScore(Aukcja auction, List<Licytacja> bids, double pvRatio, double momentum)
        {
            double risk = 30.0; // Base baseline risk

            // High P/V ratio means low margin of safety => increases risk
            if (pvRatio > 0.9) risk += 30.0;
            else if (pvRatio > 0.75) risk += 15.0;

            // High momentum bidding wars increase overbidding risk
            if (momentum > 75.0) risk += 20.0;

            // Time risk: very close to ending with intense competition
            TimeSpan timeRemaining = auction.DataZakonczenia - DateTime.Now;
            if (timeRemaining.TotalMinutes < 15 && bids.Count > 5) risk += 15.0;

            // Strategy adjustments
            if (Settings.Strategy == "Konserwatywna") risk += 10.0;
            else if (Settings.Strategy == "Agresywna") risk -= 10.0;

            return Math.Clamp(risk, 0.0, 100.0);
        }

        private string DetermineSignal(decimal roi, double pvRatio, double risk, string strategy, decimal targetRoi)
        {
            double maxAllowedRisk = strategy switch
            {
                "Konserwatywna" => 45.0,
                "Agresywna" => 80.0,
                _ => 65.0 // Zrównoważona
            };

            if (risk > maxAllowedRisk)
            {
                return "OMIJAJ (Wysokie Ryzyko)";
            }

            if (roi >= targetRoi + 15m && pvRatio <= 0.7)
            {
                return "KUP (Silny Sygnał)";
            }

            if (roi >= targetRoi)
            {
                return "KUP";
            }

            if (roi > 0m)
            {
                return "NEUTRALNY";
            }

            return "OMIJAJ";
        }

        private async Task ExecuteInvestmentDecisionsAsync(List<AuctionAnalysis> analyses)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return;

            var opportunities = analyses
                .Where(a => a.Signal.StartsWith("KUP"))
                .Where(a => a.NextBidAmount <= Settings.MaxBidLimit)
                .Where(a => a.Auction.IdUzytkownika != currentUser.IdUzytkownika) // Don't bid on own auction
                .ToList();

            foreach (var opp in opportunities)
            {
                var highestBidderId = opp.Auction.Licytacje?
                    .OrderByDescending(l => l.KwotaLicytacji)
                    .FirstOrDefault()?.IdUzytkownika;

                // Don't re-bid if current user is already the top bidder
                if (highestBidderId == currentUser.IdUzytkownika)
                {
                    continue;
                }

                AddLog("DECISION", $"Wykryto okazję inwestycyjną: '{opp.Auction.Tytul}' | Sygnał: {opp.Signal} | Przewidywany ROI: {opp.ExpectedRoiPercentage}% | Proponowana oferta: {opp.NextBidAmount:C2}");

                bool success = await _auctionService.PlaceBidAsync(opp.Auction.IdAukcji, currentUser.IdUzytkownika, opp.NextBidAmount);

                if (success)
                {
                    AddLog("EXECUTION", $"[SUKCES] Automatycznie złożono ofertę {opp.NextBidAmount:C2} dla aukcji '{opp.Auction.Tytul}' (ID: {opp.Auction.IdAukcji}).");
                }
                else
                {
                    AddLog("WARNING", $"[NIEPOWODZENIE] Nie udało się złożyć oferty {opp.NextBidAmount:C2} dla aukcji '{opp.Auction.Tytul}'.");
                }
            }
        }

        private void AddLog(string level, string message)
        {
            var entry = new BotLogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            };

            lock (_lockObj)
            {
                _logs.Add(entry);
                if (_logs.Count > 200) _logs.RemoveAt(0);
            }

            OnLogAdded?.Invoke(entry);
        }
    }
}
