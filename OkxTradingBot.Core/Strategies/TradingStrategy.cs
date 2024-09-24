using Newtonsoft.Json;
using OkxTradingBot.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OkxTradingBot.Core.Strategies
{
    public class TradingStrategy
    {
        private readonly OkxApiClient _apiClient;
        private readonly HttpAPI _http;

        public TradingStrategy(OkxApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // 筛选值得交易的虚拟币
        public async Task<List<string>> GetWorthTradingCoins()
        {
            var coins = await _http.GetTopCoinsByVolumeAsync(); // 获取24小时交易量榜单
            return coins.Where(coin => coin.Volume > 1000 && coin.GainPercentage > 2)
                        .Select(coin => coin.Symbol)
                        .ToList();
        }

        // 判断是否为Doji蜡烛图形态
        public bool IsDojiPattern(List<Candle> candles)
        {
            var lastCandle = candles.Last();
            return Math.Abs(lastCandle.Open - lastCandle.Close) < (decimal)((double)lastCandle.High * 0.001);
        }

        // 计算MACD和信号线
        public (decimal macd, decimal signal) CalculateMACD(List<Candle> candles)
        {
            var shortEMA = CalculateEMA(candles, 12);
            var longEMA = CalculateEMA(candles, 26);
            var macd = shortEMA - longEMA;
            var signal = CalculateEMA(macd, 9);
            return (macd, signal);
        }

        // 判断是否为MACD买入信号
        public bool IsMACDBuySignal(List<Candle> candles)
        {
            var (macd, signal) = CalculateMACD(candles);
            return macd > signal; // 买入信号
        }

        // 执行交易策略
        public async Task ExecuteTradingStrategy(decimal amount)
        {
            var coins = await GetWorthTradingCoins();
            foreach (var coin in coins)
            {
                var candles = await GetCandlestickDataAsync(coin);

                if (IsDojiPattern(candles) || IsMACDBuySignal(candles))
                {
                    // 满足条件则执行买入操作
                    await _apiClient.PlaceBuyOrderAsync(coin, amount);
                }
                else
                {
                    // 不满足条件，可能不操作或卖出
                }
            }
        }

        // 简单EMA计算 (这是个伪代码, 你需要根据实际计算逻辑实现)
        private decimal CalculateEMA(IEnumerable<Candle> candles, int period)
        {
            // 计算移动平均线的逻辑
            return candles.Take(period).Average(c => c.Close);
        }

        private decimal CalculateEMA(decimal macd, int period)
        {
            // 简单信号线计算
            return macd;
        }

        public async Task<List<Candle>> GetCandlestickDataAsync(string symbol)
        {
            var url = $"https://example.com/api/market/candles?symbol={symbol}";
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);

            // 假设数据是JSON格式的，解析成List<Candle>
            var candles = JsonConvert.DeserializeObject<List<Candle>>(response);
            return candles;
        }

    }

    public class Candle
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public DateTime Time { get; set; }
    }

}
