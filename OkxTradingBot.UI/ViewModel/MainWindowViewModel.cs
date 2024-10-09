using ccxt;
using CommunityToolkit.Mvvm.Input;
using OkxTradingBot.Core.Api;
using OkxTradingBot.Core.Strategies;
using OkxTradingBot.Core.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace OkxTradingBot.UI.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private OkxApiClient _apiClient;
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

        private readonly TradingStrategy _tradingStrategy;

        private readonly CryptoDataFetcher fetcher;

        public MainWindowViewModel()
        {
            _apiClient = new OkxApiClient();
            _tradingStrategy = new TradingStrategy(_apiClient);
            StartTradingCommand = new RelayCommand(StartTrading);
            ExportCommand = new RelayCommand(ExportToExcel);
            fetcher = new CryptoDataFetcher();
        }

        // 调用策略类的交易策略
        private async void StartTrading()
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
            }

            var hotCryptos = await fetcher.FetchHotCryptoDataAsync();
            var GainersLosers = await fetcher.FetchGainersLosersDataAsync();
            var LosersGainers = await fetcher.FetchLosersGainersDataAsync();
            var NewCrypto = await fetcher.FetchNewCryptoDataAsync();
            var TrendingCrypto = await fetcher.FetchTrendingCryptoDataAsync();
            var CryptoMarketCap = await fetcher.FetchCryptoMarketCapDataAsync();
            var CryptoVolume = await fetcher.FetchCryptoVolumeDataAsync();

            // 获取账户USDT余额
            decimal BTC = await _apiClient.GetBalanceAsync("BTC");
            Debug.WriteLine($"账户BTC余额: {BTC}");
            // 如果有足够的USDT，进行买单操作
            if ((double)BTC > 0.5)
            {
                string orderResult = await _apiClient.PlaceSellOrderAsync("BTC/USDT", 0.5m);
                Debug.WriteLine($"订单号:{orderResult}");
                string orderState = await _apiClient.GetOrderStatusAsync("BTC/USDT", orderResult);
                Debug.WriteLine($"订单状态:{orderState}");
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
