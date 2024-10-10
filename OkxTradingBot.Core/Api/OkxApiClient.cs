using ccxt;
using OkxTradingBot.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace OkxTradingBot.Core.Api
{
    /// <summary>
    /// 封装 OKX API 调用逻辑
    /// </summary>
    public class OkxApiClient
    {
        private readonly okx _Api;

        public OkxApiClient()
        {
            // 从配置文件中获取 API 密钥、私钥和 passphrase
            string apiKey = AppConfig.GetApiKey();
            string secretKey = AppConfig.GetSecretKey();
            string passphrase = AppConfig.GetPassphrase();

            _Api = new okx();
            _Api.apiKey = apiKey;
            _Api.secret = secretKey;
            _Api.password = passphrase;
            _Api.setSandboxMode(true); // 启用沙箱模式，用于测试交易
        }

        /// <summary>
        /// 获取所有交易对
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllSymbolsAsync()
        {
            var markets = await _Api.LoadMarkets();
            List<string> symbols = new List<string>();

            foreach (var market in markets)
            {
                symbols.Add(market.Value.symbol);
            }

            return symbols;
        }

        /// <summary>
        /// 获取指定交易对的当前价格
        /// </summary>
        /// <param name="symbol">交易对符号，如 "BTC/USDT"</param>
        /// <returns>返回交易对的最新价格</returns>
        public async Task<decimal> GetPriceAsync(string symbol)
        {
            var ticker = await _Api.FetchTicker(symbol);
            return (decimal)(ticker.last != null ? ticker.last : 0);
        }

        /// <summary>
        /// 提交市价买单
        /// </summary>
        /// <param name="symbol">交易对符号</param>
        /// <param name="amount">买入数量</param>
        /// <returns>返回是否成功下单</returns>
        public async Task<string> PlaceBuyOrderAsync(string symbol, decimal amount)
        {
            var order = await _Api.CreateMarketBuyOrder(symbol, (double)amount);
            return order.id;
        }

        /// <summary>
        /// 获取账户余额
        /// </summary>
        /// <param name="currency">货币名称，如 "USDT"</param>
        /// <returns>返回账户中该货币的余额</returns>
        public async Task<decimal> GetBalanceAsync(string currency)
        {
            var balance = await _Api.FetchBalance();
            return balance[currency].free != null ? (decimal)balance[currency].free : 0;
        }

        /// <summary>
        /// 批量获取多个交易对的价格
        /// </summary>
        /// <param name="symbols">交易对列表</param>
        /// <returns>返回每个交易对的最新价格</returns>
        public async Task<Dictionary<string, decimal>> GetMultiplePricesAsync(List<string> symbols)
        {
            var prices = new Dictionary<string, decimal>();

            foreach (var symbol in symbols)
            {
                var price = await GetPriceAsync(symbol);
                prices[symbol] = price;
            }

            return prices;
        }

        /// <summary>
        /// 提交市价卖单
        /// </summary>
        /// <param name="symbol">交易对符号</param>
        /// <param name="amount">卖出数量</param>
        /// <returns>返回是否成功下单</returns>
        public async Task<string> PlaceSellOrderAsync(string symbol, decimal amount)
        {
            var order = await _Api.CreateMarketSellOrder(symbol, (double)amount);
            return order.id;
        }

        /// <summary>
        /// 追踪订单状态
        /// </summary>
        /// <param name="symbol">交易对，例如 "BTC/USDT"</param>
        /// <param name="orderId">订单ID</param>
        /// <returns>返回订单状态</returns>
        public async Task<string> GetOrderStatusAsync(string symbol, string orderId)
        {
            // 通过订单ID获取订单详情
            var order = await _Api.FetchOrder(orderId, symbol);

            // 返回订单状态，例如 "open", "closed", "canceled", "partially_filled"
            return order.status;
        }

        /// <summary>
        /// 获取指定交易对和时间间隔的OHLCV数据
        /// </summary>
        /// <param name="subscriptions">交易对及其时间间隔列表</param>
        public async Task WatchOHLCVForSymbols(List<List<string>> subscriptions)
        {
            while (true)
            {
                try
                {
                    // 使用ccxt.pro获取OHLCV实时数据
                    var ohlcv = await _Api.WatchOHLCVForSymbols(subscriptions);
                    foreach (var ohlcvData in ohlcv)
                    {
                        Console.WriteLine($"交易对: {ohlcvData.Key}, 数据: {ohlcvData.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"获取OHLCV数据时发生错误: {ex.Message}");
                }
            }
        }

        public class MACDResult
        {
            public decimal MACDLine { get; set; }
            public decimal SignalLine { get; set; }
            public decimal Histogram { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="prices"></param>
        /// <param name="fastPeriod"></param>
        /// <param name="slowPeriod"></param>
        /// <param name="signalPeriod"></param>
        /// <returns></returns>
        /// 
        //        MACD 计算公式：

        //MACD线 = 12日EMA - 26日EMA
        //信号线 = MACD线的9日EMA
        //柱状图 = MACD线 - 信号线

        public List<MACDResult> CalculateMACD(List<decimal> prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            List<MACDResult> macdResults = new List<MACDResult>();

            // 计算快速和慢速 EMA
            var fastEMA = CalculateEMA(prices, fastPeriod);
            var slowEMA = CalculateEMA(prices, slowPeriod);

            // 计算MACD线
            List<decimal> macdLine = new List<decimal>();
            for (int i = 0; i < prices.Count; i++)
            {
                if (i >= slowPeriod - 1) // 确保有足够的数据来计算慢速EMA
                {
                    macdLine.Add(fastEMA[i] - slowEMA[i]);
                }
                else
                {
                    macdLine.Add(0); // 尚未有足够数据计算
                }
            }

            // 计算信号线
            var signalLine = CalculateEMA(macdLine, signalPeriod);

            // 计算柱状图
            for (int i = 0; i < prices.Count; i++)
            {
                var macdResult = new MACDResult
                {
                    MACDLine = macdLine[i],
                    SignalLine = signalLine[i],
                    Histogram = macdLine[i] - signalLine[i]
                };
                macdResults.Add(macdResult);
            }

            return macdResults;
        }

        private List<decimal> CalculateEMA(List<decimal> prices, int period)
        {
            List<decimal> ema = new List<decimal>();
            decimal multiplier = 2m / (period + 1);

            // 初始的EMA使用简单移动平均计算
            decimal sma = prices.Take(period).Average();
            ema.Add(sma);

            // 后续的EMA根据公式计算
            for (int i = period; i < prices.Count; i++)
            {
                decimal value = ((prices[i] - ema.Last()) * multiplier) + ema.Last();
                ema.Add(value);
            }

            return ema;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prices"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        /// 
        //        RSI 计算公式：

        //RSI = 100 - (100 / (1 + RS))
        //其中，RS = 平均上涨 / 平均下跌
        public List<decimal> CalculateRSI(List<decimal> prices, int period = 14)
        {
            List<decimal> rsiValues = new List<decimal>();
            decimal gainSum = 0;
            decimal lossSum = 0;

            // 初始化，计算初始的平均涨跌幅
            for (int i = 1; i <= period; i++)
            {
                var change = prices[i] - prices[i - 1];
                if (change > 0)
                    gainSum += change;
                else
                    lossSum -= change; // 损失是负值，所以取相反数
            }

            // 计算第一个 RSI
            decimal avgGain = gainSum / period;
            decimal avgLoss = lossSum / period;
            rsiValues.Add(CalculateRS(avgGain, avgLoss));

            // 逐步计算后续 RSI
            for (int i = period + 1; i < prices.Count; i++)
            {
                var change = prices[i] - prices[i - 1];
                if (change > 0)
                {
                    avgGain = ((avgGain * (period - 1)) + change) / period;
                    avgLoss = ((avgLoss * (period - 1))) / period; // 无跌幅时，平均损失保持不变
                }
                else
                {
                    avgGain = (avgGain * (period - 1)) / period;
                    avgLoss = ((avgLoss * (period - 1)) - change) / period;
                }
                rsiValues.Add(CalculateRS(avgGain, avgLoss));
            }

            return rsiValues;
        }

        private decimal CalculateRS(decimal avgGain, decimal avgLoss)
        {
            if (avgLoss == 0) return 100; // 避免除以0
            decimal rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }


    }
}
