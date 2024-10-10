using ccxt;
using CommunityToolkit.Mvvm.Input;
using OkxTradingBot.Core.Api;
using OkxTradingBot.Core.Strategies;
using OkxTradingBot.Core.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace OkxTradingBot.UI.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private OkxApiClient _apiClient;
        private DispatcherTimer _priceTimer;

        // 绑定的交易对集合
        public ObservableCollection<string> Symbols { get; set; }

        public ObservableCollection<Core.Orders.Order> Orders { get; set; } = new ObservableCollection<Core.Orders.Order>();

        public ObservableCollection<string> CandidateSymbols { get; set; }

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

            // 初始化定时器
            _priceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30) // 每隔 30 秒获取一次价格
            };
            _priceTimer.Tick += (s, e) => FetchPriceAsync();
            _priceTimer.Start();

            LoadSymbols();

            CandidateSymbols = new ObservableCollection<string>();
            LoadCandidateSymbols();
        }

        private async void LoadCandidateSymbols()
        {
            // 获取数据并根据 ChanLunStrategy 进行筛选
            var fetcher = new CryptoDataFetcher();
            var allSymbols = await fetcher.FetchAllSymbolsAsync(); // 获取所有币种

            // 假设有一个方法来筛选和排序
            var filteredSymbols = await ChanLunStrategy.AnalyzeSymbolsAsync(allSymbols);

            // 清空当前候选币种并添加新的
            CandidateSymbols.Clear();
            foreach (var symbol in filteredSymbols)
            {
                CandidateSymbols.Add(symbol);
            }
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
                   && (symbol.EndsWith("/OKB") || symbol.EndsWith("/USDT"));
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

        public async Task MonitorOHLCVData()
        {
            // 订阅交易对和时间间隔，例如：BTC/USDT 5分钟 和 ETH/USDT 5分钟
            var subscriptions = new List<List<string>>()
            {
                new List<string> { "BTC/USDT", "5m" },
                new List<string> { "ETH/USDT", "5m" },
                new List<string> { "BTC/USDT", "1h" }
            };

            // 开始监听OHLCV数据
            await _apiClient.WatchOHLCVForSymbols(subscriptions);
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
