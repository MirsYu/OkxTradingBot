using ccxt;
using CommunityToolkit.Mvvm.Input;
using OkxTradingBot.Core.Api;
using OkxTradingBot.Core.Strategies;
using OkxTradingBot.Core.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace OkxTradingBot.UI.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private OkxApiClient _apiClient;
        private DispatcherTimer _priceTimer;

        private bool _isExecuting = false; // 标志位

        private const int MaxLogLines = 100;

        // 绑定的交易对集合
        public ObservableCollection<string> Symbols { get; set; }

        public ObservableCollection<Core.Orders.Order> Orders { get; set; } = new ObservableCollection<Core.Orders.Order>();

        public ObservableCollection<CryptoHotData> HotSymbols { get; set; } = new ObservableCollection<CryptoHotData>();
        public ObservableCollection<CryptoGainersLosersData> GainersSymbols { get; set; } = new ObservableCollection<CryptoGainersLosersData>();
        public ObservableCollection<CryptoLosersGainersData> LosersSymbols { get; set; } = new ObservableCollection<CryptoLosersGainersData>();
        public ObservableCollection<CryptoNewData> NewSymbols { get; set; } = new ObservableCollection<CryptoNewData>();
        public ObservableCollection<TrendingCrypto> TrendingSymbols { get; set; } = new ObservableCollection<TrendingCrypto>();
        public ObservableCollection<CryptoMarketCap> MarketCapSymbols { get; set; } = new ObservableCollection<CryptoMarketCap>();
        public ObservableCollection<CryptoVolumeData> VolumeSymbols { get; set; } = new ObservableCollection<CryptoVolumeData>();

        private string _tradeSignal;
        public string TradeSignal
        {
            get => _tradeSignal;
            set
            {
                _tradeSignal = value;
                OnPropertyChanged(nameof(TradeSignal));
                // 在设置信号后触发事件
                OnTradeSignalChanged();
            }
        }

        // 用于处理交易信号变化的事件
        public event EventHandler<string> TradeSignalChanged;

        // 触发交易信号变化事件
        private void OnTradeSignalChanged()
        {
            TradeSignalChanged?.Invoke(this, TradeSignal);
        }

        private ObservableCollection<string> _logEntries;

        public ObservableCollection<string> LogEntries
        {
            get => _logEntries;
            set
            {
                _logEntries = value;
                OnPropertyChanged(nameof(LogEntries));
            }
        }



        // 新增一个方法来更新日志
        private void UpdateLog(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            // 添加新日志项
            LogEntries.Add(logEntry);

            // 保持最多 100 行
            if (LogEntries.Count > MaxLogLines)
            {
                LogEntries.RemoveAt(0); // 删除最早的行
            }
        }


        private string _selectedSymbol;
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set
            {
                _selectedSymbol = value;
                OnPropertyChanged(nameof(SelectedSymbol));
                FetchPriceAsync(); // 每次选择交易对时获取价格
            }
        }

        private decimal _currentPrice;
        public decimal CurrentPrice
        {
            get => _currentPrice;
            set
            {
                _currentPrice = value;
                OnPropertyChanged(nameof(CurrentPrice));
            }
        }


        public ICommand StartTradingCommand { get; }

        public ICommand ExportCommand { get; }

        private readonly CryptoDataFetcher fetcher;

        public MainWindowViewModel()
        {
            _apiClient = new OkxApiClient();
            StartTradingCommand = new RelayCommand(StartTrading);
            ExportCommand = new RelayCommand(ExportToExcel);
            fetcher = new CryptoDataFetcher();
            Symbols = new ObservableCollection<string>();
            LogEntries = new ObservableCollection<string>();

            LoadSymbols();

            // 初始化定时器
            _priceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5) // 每隔 30 秒获取一次价格
            };
            _priceTimer.Tick += async (s, e) =>
            {
                if (!_isExecuting) // 只有在没有执行时才开始新的任务
                {
                    _isExecuting = true;
                    await LoadCandidateSymbols(); // 加载候选币种
                    _isExecuting = false;
                }
            };
            _priceTimer.Start();
        }


        private async Task LoadCandidateSymbols()
        {
            var hotCryptos = await fetcher.FetchHotCryptoDataAsync();
            var gainersLosers = await fetcher.FetchGainersLosersDataAsync();
            var losersGainers = await fetcher.FetchLosersGainersDataAsync();
            var newCrypto = await fetcher.FetchNewCryptoDataAsync();
            var trendingCrypto = await fetcher.FetchTrendingCryptoDataAsync();
            var cryptoMarketCap = await fetcher.FetchCryptoMarketCapDataAsync();
            var cryptoVolume = await fetcher.FetchCryptoVolumeDataAsync();

            // 清空并添加新的数据
            HotSymbols.Clear();
            foreach (var crypto in hotCryptos)
            {
                HotSymbols.Add(crypto);
            }

            GainersSymbols.Clear();
            foreach (var gainer in gainersLosers)
            {
                GainersSymbols.Add(gainer);
            }

            LosersSymbols.Clear();
            foreach (var loser in losersGainers)
            {
                LosersSymbols.Add(loser);
            }

            NewSymbols.Clear();
            foreach (var newC in newCrypto)
            {
                NewSymbols.Add(newC);
            }

            TrendingSymbols.Clear();
            foreach (var trend in trendingCrypto)
            {
                TrendingSymbols.Add(trend);
            }

            MarketCapSymbols.Clear();
            foreach (var marketCap in cryptoMarketCap)
            {
                MarketCapSymbols.Add(marketCap);
            }

            VolumeSymbols.Clear();
            foreach (var volume in cryptoVolume)
            {
                VolumeSymbols.Add(volume);
            }

            // 在数据加载完毕后，开始监控所有符号
            await MonitorOHLCVForAllSymbols("15m");
        }

        public async Task MonitorOHLCVForAllSymbols(string interval)
        {
            // 合并所有币种列表并提取 Symbol
            var allSymbols = HotSymbols.Select(symbolData => symbolData.Symbol)
                .Concat(GainersSymbols.Select(symbolData => symbolData.Symbol))
                .Concat(LosersSymbols.Select(symbolData => symbolData.Symbol))
                .Concat(NewSymbols.Select(symbolData => symbolData.Symbol))
                .Concat(MarketCapSymbols.Select(symbolData => symbolData.Symbol))
                .Concat(VolumeSymbols.Select(symbolData => symbolData.Symbol))
                .Concat(TrendingSymbols.Select(symbolData => symbolData.Symbol))
                .Distinct(); // 去重

            // 遍历去重后的币种列表
            foreach (var symbol in allSymbols)
            {
                await MonitorOHLCVData(symbol, interval);
            }
        }

        // 在 MonitorOHLCVData 方法中
        public async Task MonitorOHLCVData(string symbol, string interval)
        {
            var candles = await _apiClient.FetchOHLCVData(symbol, interval);

            if (candles.Count < 26)
            {
                Debug.WriteLine("蜡烛数据不足，无法计算MACD。");
                return;
            }

            var closingPrices = candles.Select(c => c.Close).ToList();
            var strategy = new ChanLunStrategy();
            var macdResults = _apiClient.CalculateMACD(closingPrices);

            // 提取MACD值
            var macdValues = macdResults.Select(m => m.MACDLine).ToList();

            // 只获取最新蜡烛
            var latestCandle = candles.Last();
            var latestMacdValue = macdValues.Last();

            // 生成信号，仅基于最新蜡烛和历史计算的指标
            string signal = strategy.GenerateSignal(candles, macdValues);

            // 设置 TradeSignal 属性

            if (signal != "Hold")
            {
                try
                {
                    CurrentPrice = await _apiClient.GetPriceAsync(symbol);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"获取价格时出错: {ex.Message}");
                }

                TradeSignal = $"{symbol}：方向: {signal} 价格：{CurrentPrice}";
            }
            else
            {
                TradeSignal = $"{symbol}：方向: {signal}";
            }

            // 更新日志
            UpdateLog(TradeSignal);
        }


        // 加载交易对的方法
        private async Task LoadSymbols()
        {
            try
            {
                // 获取所有交易对
                var symbols = await _apiClient.GetAllSymbolsAsync();

                Symbols.Clear();

                // 过滤掉不需要的交易对
                foreach (var symbol in symbols)
                {
                    // 检查交易对格式是否为 "BASE/QUOTE"
                    if (IsSpotPair(symbol))
                    {
                        Symbols.Add(symbol);
                    }
                }

                // 默认选择第一个交易对
                SelectedSymbol = Symbols.Count > 0 ? Symbols[0] : null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"获取交易对时出错: {ex.Message}");
            }
        }

        private bool IsSpotPair(string symbol)
        {
            // 仅选择 "OKB" 或 "USDT" 为 QUOTE 的交易对
            return !symbol.Contains(":") && !symbol.Contains("-") && !symbol.Contains(".")
                   && /*(symbol.EndsWith("/OKB") ||*/ symbol.EndsWith("/USDT");
        }


        // 获取选定交易对的价格
        private async Task FetchPriceAsync()
        {
            if (string.IsNullOrEmpty(SelectedSymbol)) return;

            try
            {
                CurrentPrice = await _apiClient.GetPriceAsync(SelectedSymbol);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"获取价格时出错: {ex.Message}");
            }
        }

        // 调用策略类的交易策略
        private async void StartTrading()
        {
            try
            {
                // 获取当前BTC价格
                decimal btcPrice = await _apiClient.GetPriceAsync("BTC/USDT");
                CurrentPrice = btcPrice;
                Debug.WriteLine($"当前BTC价格: {btcPrice}");

                // 获取账户USDT余额
                decimal usdtBalance = await _apiClient.GetBalanceAsync("USDT");
                Debug.WriteLine($"账户USDT余额: {usdtBalance}");

                // 如果有足够的USDT，进行买单操作
                if (usdtBalance > 100)
                {
                    string orderResult = await _apiClient.PlaceBuyOrderAsync("BTC/USDT", 0.5m);
                    Debug.WriteLine($"订单号:{orderResult}");
                    string orderState = await _apiClient.GetOrderStatusAsync("BTC/USDT", orderResult);
                    Debug.WriteLine($"订单状态:{orderState}");

                    // 添加订单到 Orders 集合
                    Orders.Add(new Core.Orders.Order
                    {
                        OrderId = orderResult,
                        Symbol = "BTC/USDT",
                        Side = "买单",
                        Status = orderState,
                        Amount = 0.5m,
                        Price = CurrentPrice,
                        OrderTime = DateTime.Now // 当前时间
                    });
                }

                // 获取其他数据
                var hotCryptos = await fetcher.FetchHotCryptoDataAsync();
                var gainersLosers = await fetcher.FetchGainersLosersDataAsync();
                var losersGainers = await fetcher.FetchLosersGainersDataAsync();
                var newCrypto = await fetcher.FetchNewCryptoDataAsync();
                var trendingCrypto = await fetcher.FetchTrendingCryptoDataAsync();
                var cryptoMarketCap = await fetcher.FetchCryptoMarketCapDataAsync();
                var cryptoVolume = await fetcher.FetchCryptoVolumeDataAsync();

                // 获取账户BTC余额
                decimal BTC = await _apiClient.GetBalanceAsync("BTC");
                Debug.WriteLine($"账户BTC余额: {BTC}");
                // 如果有足够的BTC，进行卖单操作
                if (BTC > 0.5m)
                {
                    string sellOrderResult = await _apiClient.PlaceSellOrderAsync("BTC/USDT", 0.5m);
                    Debug.WriteLine($"订单号:{sellOrderResult}");

                    string sellOrderState = await _apiClient.GetOrderStatusAsync("BTC/USDT", sellOrderResult);
                    Debug.WriteLine($"订单状态:{sellOrderState}");

                    // 获取成交价格
                    decimal sellPrice = CurrentPrice; // 根据实际逻辑获取价格

                    // 将订单信息添加到 Orders 集合中
                    Orders.Add(new Core.Orders.Order
                    {
                        OrderId = sellOrderResult,
                        Symbol = "BTC/USDT",
                        Side = "卖单",
                        Status = sellOrderState,
                        Amount = 0.5m,
                        Price = sellPrice,
                        OrderTime = DateTime.Now // 当前时间

                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"交易过程中出现错误: {ex.Message}");
            }
        }

        private void ExportToExcel()
        {
            var exporter = new ExcelExporter();
            List<Order> _orders = new List<Order>();
            exporter.ExportOrdersToExcel(_orders, "Orders.xlsx");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
