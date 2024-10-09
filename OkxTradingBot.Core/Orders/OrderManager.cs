using OkxTradingBot.Core.Api;
using System;
using System.Threading.Tasks;

namespace OkxTradingBot.Core.Orders
{
    /// <summary>
    /// 订单管理模块
    /// 管理订单状态和下单逻辑
    /// </summary>
    public class OrderManager
    {
        private readonly OkxApiClient _apiClient;

        public OrderManager(OkxApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task PlaceOrderAsync(string symbol, decimal amount)
        {
            string result = await _apiClient.PlaceBuyOrderAsync(symbol, amount);
            if (result!= null)
            {
                Console.WriteLine("Order placed successfully.");
            }
            else
            {
                Console.WriteLine("Failed to place order.");
            }
        }
    }
}
