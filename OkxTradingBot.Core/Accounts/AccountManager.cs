using ccxt;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace OkxTradingBot.Core.Accounts
{
    /// <summary>
    /// 账户管理模块
    /// 查询账户余额和持仓
    /// </summary>
    public class AccountManager
    {
        private readonly okx _privateApi;

        public AccountManager(okx privateApi)
        {
            _privateApi = privateApi;
        }

        public async Task<decimal> GetBalanceAsync(string currency)
        {
            var balance = await _privateApi.FetchBalance();
            Console.WriteLine(JsonConvert.SerializeObject(balance, Formatting.Indented));
            return /*balance?.balances[currency] ?? */0;
        }
    }
}
