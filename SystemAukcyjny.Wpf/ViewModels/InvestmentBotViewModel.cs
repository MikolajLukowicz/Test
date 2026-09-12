using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SystemAukcyjny.Wpf.Models;
using SystemAukcyjny.Wpf.Services;

namespace SystemAukcyjny.Wpf.ViewModels
{
    public partial class InvestmentBotViewModel : ViewModelBase
    {
        private readonly IInvestmentBotService _botService;

        [ObservableProperty]
        private bool _isBotRunning;

        [ObservableProperty]
        private decimal _maxBidLimit = 1000m;

        [ObservableProperty]
        private decimal _targetMinRoiPercentage = 15.0m;

        [ObservableProperty]
        private int _scanIntervalSeconds = 5;

        [ObservableProperty]
        private string _selectedStrategy = "Zrównoważona";

        [ObservableProperty]
        private bool _autoBidEnabled = true;

        [ObservableProperty]
        private string _statusText = "Bot zatrzymany";

        [ObservableProperty]
        private int _totalAnalyzedCount;

        [ObservableProperty]
        private int _activeBuySignalsCount;

        [ObservableProperty]
        private int _totalBidsExecutedCount;

        public ObservableCollection<string> Strategies { get; } = new()
        {
            "Konserwatywna",
            "Zrównoważona",
            "Agresywna"
        };

        public ObservableCollection<AuctionAnalysis> Analyses { get; } = new();
        public ObservableCollection<BotLogEntry> Logs { get; } = new();

        public InvestmentBotViewModel(IInvestmentBotService botService)
        {
            _botService = botService;

            // Sync properties with service settings
            MaxBidLimit = _botService.Settings.MaxBidLimit;
            TargetMinRoiPercentage = _botService.Settings.TargetMinRoiPercentage;
            ScanIntervalSeconds = _botService.Settings.ScanIntervalSeconds;
            SelectedStrategy = _botService.Settings.Strategy;
            AutoBidEnabled = _botService.Settings.AutoBidEnabled;
            IsBotRunning = _botService.IsRunning;

            _botService.OnAnalysisUpdated += BotService_OnAnalysisUpdated;
            _botService.OnLogAdded += BotService_OnLogAdded;
            _botService.OnRunningStateChanged += BotService_OnRunningStateChanged;

            // Load initial state
            RefreshLogs();
            RefreshAnalyses(_botService.CurrentAnalyses);
            UpdateStatusText();
        }

        private void BotService_OnRunningStateChanged(bool isRunning)
        {
            ExecuteOnUIThread(() =>
            {
                IsBotRunning = isRunning;
                UpdateStatusText();
            });
        }

        private void BotService_OnLogAdded(BotLogEntry entry)
        {
            ExecuteOnUIThread(() =>
            {
                Logs.Insert(0, entry);
                if (Logs.Count > 200) Logs.RemoveAt(Logs.Count - 1);

                if (entry.Level == "EXECUTION")
                {
                    TotalBidsExecutedCount++;
                }
            });
        }

        private void BotService_OnAnalysisUpdated(System.Collections.Generic.IEnumerable<AuctionAnalysis> analyses)
        {
            ExecuteOnUIThread(() =>
            {
                RefreshAnalyses(analyses);
            });
        }

        private void RefreshAnalyses(System.Collections.Generic.IEnumerable<AuctionAnalysis> analyses)
        {
            Analyses.Clear();
            var list = analyses.ToList();
            foreach (var item in list)
            {
                Analyses.Add(item);
            }

            TotalAnalyzedCount = list.Count;
            ActiveBuySignalsCount = list.Count(a => a.Signal.StartsWith("KUP"));
        }

        private void RefreshLogs()
        {
            Logs.Clear();
            foreach (var entry in _botService.Logs.OrderByDescending(l => l.Timestamp))
            {
                Logs.Add(entry);
            }
        }

        private void UpdateStatusText()
        {
            StatusText = IsBotRunning ? "🟢 Bot aktywny - analizowanie rynku..." : "🔴 Bot zatrzymany";
        }

        [RelayCommand]
        private async Task ToggleBot()
        {
            ApplySettingsToService();

            if (IsBotRunning)
            {
                await _botService.StopAsync();
            }
            else
            {
                await _botService.StartAsync();
            }
        }

        [RelayCommand]
        private async Task RunManualAnalysis()
        {
            ApplySettingsToService();
            StatusText = "⏳ Analizowanie rynku...";
            var analyses = await _botService.RunSingleAnalysisCycleAsync();
            RefreshAnalyses(analyses);
            UpdateStatusText();
        }

        [RelayCommand]
        private void ApplySettings()
        {
            ApplySettingsToService();
            StatusText = "⚙️ Ustawienia bota zaktualizowane.";
        }

        [RelayCommand]
        private void ClearLogs()
        {
            Logs.Clear();
        }

        private void ApplySettingsToService()
        {
            _botService.Settings.MaxBidLimit = MaxBidLimit;
            _botService.Settings.TargetMinRoiPercentage = TargetMinRoiPercentage;
            _botService.Settings.ScanIntervalSeconds = Math.Max(1, ScanIntervalSeconds);
            _botService.Settings.Strategy = SelectedStrategy;
            _botService.Settings.AutoBidEnabled = AutoBidEnabled;
        }

        private static void ExecuteOnUIThread(Action action)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(action);
                }
            }
            else
            {
                action();
            }
        }
    }
}
