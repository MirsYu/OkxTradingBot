using ccxt;
using OkxTradingBot.Core.Config;
using OkxTradingBot.Core.Strategies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace OkxTradingBot.Core.Api
{
    /// <summary>
    /// 封装 OKX API 调用逻辑
    /// </summary>
    public class OkxApiClient
    {
        private readonly okx _Api;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10); // 同时允许最多10个请求
        private static readonly TimeSpan RateLimitPeriod = TimeSpan.FromSeconds(2); // 每2秒
        private static DateTime _lastRequestTime = DateTime.MinValue;

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

        // 请求速率限制器
        private async Task RateLimitAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                var now = DateTime.UtcNow;
                var timeSinceLastRequest = now - _lastRequestTime;

                if (timeSinceLastRequest < RateLimitPeriod)
                {
                    await Task.Delay(RateLimitPeriod - timeSinceLastRequest); // 等待剩余时间
                }

                _lastRequestTime = DateTime.UtcNow; // 更新最后一次请求时间
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 获取所有交易对
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetAllSymbolsAsync()
        {
            await RateLimitAsync(); // 调用速率限制
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
            await RateLimitAsync(); // 调用速率限制
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
            await RateLimitAsync(); // 调用速率限制
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
            await RateLimitAsync(); // 调用速率限制
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
            await RateLimitAsync(); // 调用速率限制
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
            await RateLimitAsync(); // 调用速率限制
            // 通过订单ID获取订单详情
            var order = await _Api.FetchOrder(orderId, symbol);

            // 返回订单状态，例如 "open", "closed", "canceled", "partially_filled"
            return order.status;
        }

        /// <summary>
        /// 获取指定交易对和时间间隔的OHLCV数据
        /// </summary>
        /// <param name="subscriptions">交易对及其时间间隔列表</param>
        public async Task<List<Candle>> FetchOHLCVData(string symbol, string timeframe)
        {
            var candles = new List<Candle>(); // 用于存储蜡烛图数据
            try
            {
                // 定义所需的蜡烛数量
                int requiredCandles = 50; // 至少需要26根K线
                TimeSpan candleTimeSpan;

                // 根据时间间隔设置时间跨度（这里假设时间间隔为 "15m"）
                switch (timeframe)
                {
                    case "15m":
                        candleTimeSpan = TimeSpan.FromMinutes(15);
                        break;
                    case "1h":
                        candleTimeSpan = TimeSpan.FromHours(1);
                        break;
                    // 可以添加其他时间间隔
                    default:
                        throw new ArgumentException("不支持的时间间隔");
                }

                // 计算获取数据的开始时间
                long since = DateTimeOffset.UtcNow.Add(-candleTimeSpan * requiredCandles).ToUnixTimeMilliseconds();

                // 获取历史 OHLCV 数据
                var limit = requiredCandles; // 设置获取的K线数量为所需数量
                await RateLimitAsync(); // 调用速率限制
                var ohlcv = await _Api.fetchOHLCV(symbol, timeframe, since, limit);

                if (ohlcv is List<object> ohlcvList)
                {
                    foreach (var ohlcvRow in ohlcvList)
                    {
                        if (ohlcvRow is List<object> row && row.Count >= 6)
                        {
                            var candle = new Candle
                            {
                                Time = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(row[0])).UtcDateTime.ToLocalTime(),
                                Open = Convert.ToDecimal(row[1]),
                                High = Convert.ToDecimal(row[2]),
                                Low = Convert.ToDecimal(row[3]),
                                Close = Convert.ToDecimal(row[4])
                            };
                            candles.Add(candle); // 将蜡烛图数据添加到列表中
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取OHLCV数据时发生错误: {ex.Message}");
            }

            return candles; // 返回蜡烛图数据
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
            // 检查价格数量是否足够
            if (prices.Count < slowPeriod + signalPeriod)
            {
                throw new InvalidOperationException("价格数据不足，无法计算MACD。");
            }

            List<MACDResult> macdResults = new List<MACDResult>();

            // 计算快速和慢速 EMA
            var fastEMA = CalculateEMA(prices, fastPeriod);
            var slowEMA = CalculateEMA(prices, slowPeriod);

            // 确保两者长度一致，取较小的长度
            int macdLength = Math.Min(fastEMA.Count, slowEMA.Count);

            // 计算 MACD 线
            List<decimal> macdLine = new List<decimal>();
            for (int i = 0; i < macdLength; i++)
            {
                macdLine.Add(fastEMA[i + (fastEMA.Count - macdLength)] - slowEMA[i + (slowEMA.Count - macdLength)]);
            }

            // 计算信号线
            var signalLine = CalculateEMA(macdLine, signalPeriod);

            // 计算柱状图
            for (int i = 0; i < macdLine.Count; i++)
            {
                var macdResult = new MACDResult
                {
                    MACDLine = macdLine[i],
                    SignalLine = (i < signalLine.Count) ? signalLine[i] : 0, // 如果信号线不足，默认0
                    Histogram = macdLine[i] - (i < signalLine.Count ? signalLine[i] : 0) // 如果信号线不足，默认0
                };
                macdResults.Add(macdResult);
            }

            return macdResults;
        }



        private List<decimal> CalculateEMA(List<decimal> prices, int period)
        {
            List<decimal> ema = new List<decimal>();
            decimal multiplier = 2m / (period + 1);

            // 处理不足以计算 EMA 的情况
            if (prices.Count < period)
            {
                // 如果价格数据不足以计算 EMA，返回空列表
                return ema;
            }

            // 初始的EMA使用简单移动平均计算
            decimal sma = prices.Take(period).Average();
            ema.Add(sma); // 将初始的 SMA 添加到 EMA 列表中

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
