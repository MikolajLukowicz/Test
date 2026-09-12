using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemAukcyjny.Wpf.Models;

namespace SystemAukcyjny.Wpf.Services
{
    public interface IInvestmentBotService
    {
        InvestmentBotSettings Settings { get; }
        bool IsRunning { get; }
        IReadOnlyList<BotLogEntry> Logs { get; }
        IReadOnlyList<AuctionAnalysis> CurrentAnalyses { get; }

        event Action<IEnumerable<AuctionAnalysis>>? OnAnalysisUpdated;
        event Action<BotLogEntry>? OnLogAdded;
        event Action<bool>? OnRunningStateChanged;

        Task StartAsync();
        Task StopAsync();
        Task<IEnumerable<AuctionAnalysis>> RunSingleAnalysisCycleAsync();
        AuctionAnalysis AnalyzeAuction(Aukcja auction, IEnumerable<Aukcja> categoryAuctions);
    }
}
