using System;
using System.Collections.Generic;
using System.Net.Http;
using HtmlAgilityPack; // 用于解析HTML
using System.Threading.Tasks;

namespace OkxTradingBot.Core.Api
{
    public class HttpAPI
    {
        public async Task<List<CoinInfo>> GetTopCoinsByVolumeAsync()
        {
            var url = "https://example.com/top-coins"; // 替换为实际的榜单URL
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var coins = new List<CoinInfo>();

            // 使用 HtmlAgilityPack 解析页面数据
            foreach (var node in doc.DocumentNode.SelectNodes("//table[@id='top-coins']//tr"))
            {
                var symbol = node.SelectSingleNode(".//td[1]").InnerText.Trim();
                var volume = decimal.Parse(node.SelectSingleNode(".//td[2]").InnerText.Trim());

                coins.Add(new CoinInfo { Symbol = symbol, Volume = volume });
            }

            return coins;
        }
    }
    public class CoinInfo
    {
        public string Symbol { get; set; }
        public decimal Volume { get; set; }

        public decimal GainPercentage { get; set; }
    }
}
