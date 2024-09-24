using ccxt;
using System.Threading.Tasks;

namespace OkxTradingBot.Core.Api
{
    /// <summary>
    /// 封装 OKX API 调用逻辑
    /// </summary>
    public class OkxApiClient
    {
        private readonly okx _Api;

        public OkxApiClient(string apiKey, string secretKey, string passphrase)
        {
            _Api = new okx();
            _Api.apiKey = apiKey;
            _Api.secret = secretKey;
            _Api.setSandboxMode(true);
        }

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            var ticker = await _Api.FetchTicker(symbol);
            return (decimal)(ticker.last != null ? ticker.last : 0);
        }

        public async Task<bool> PlaceBuyOrderAsync(string symbol, decimal amount)
        {
            var order = await _Api.CreateMarketBuyOrder(symbol, (double)amount);
            return order.amount != null;
        }
    }
}
