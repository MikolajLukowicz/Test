using System;
using System.Collections.Generic;

namespace SystemAukcyjny.Wpf.Models
{
    public class InvestmentBotSettings
    {
        public decimal MaxBidLimit { get; set; } = 1000m;
        public decimal TargetMinRoiPercentage { get; set; } = 15.0m;
        public int ScanIntervalSeconds { get; set; } = 5;
        public string Strategy { get; set; } = "Zrównoważona"; // "Konserwatywna", "Zrównoważona", "Agresywna"
        public bool AutoBidEnabled { get; set; } = true;
    }

    public class AuctionAnalysis
    {
        public Aukcja Auction { get; set; } = null!;
        public decimal CurrentPrice { get; set; }
        public decimal NextBidAmount { get; set; }
        public decimal EstimatedFairValue { get; set; }
        public decimal ExpectedRoiPercentage { get; set; }
        public double DemandMomentumIndex { get; set; } // 0 - 100 RSI-like indicator
        public double PriceToValueRatio { get; set; }   // P/V Ratio
        public double RiskScore { get; set; }           // 0 - 100 (Lower = safer)
        public string Signal { get; set; } = "NEUTRALNY";
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    public class BotLogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Level { get; set; } = "INFO"; // INFO, DECISION, EXECUTION, WARNING, ERROR
        public string Message { get; set; } = string.Empty;
    }
}
