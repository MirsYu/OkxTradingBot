using ccxt;
using CommunityToolkit.Mvvm.Input;
using OkxTradingBot.Core.Api;
using OkxTradingBot.Core.Strategies;
using OkxTradingBot.Core.Utils;
using System.ComponentModel;
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

        public MainWindowViewModel()
        {
            _apiClient = new OkxApiClient("apiKey", "secretKey", "passphrase");
            _tradingStrategy = new TradingStrategy(_apiClient);
            StartTradingCommand = new RelayCommand(StartTrading);
            ExportCommand = new RelayCommand(ExportToExcel);
        }

        // 调用策略类的交易策略
        private async void StartTrading()
        {
            CurrentPrice = await _apiClient.GetPriceAsync("BTC/USDT");
            await _tradingStrategy.ExecuteTradingStrategy(1);
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
