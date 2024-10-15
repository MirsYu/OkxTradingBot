using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OkxTradingBot.Core.Api
{
    public class CryptoDataFetcher
    {

        private static readonly HttpClient client = new HttpClient();

        static CryptoDataFetcher()
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer() // 初始化 CookieContainer
            };

            client = new HttpClient(handler);
        }


        public async Task<List<CryptoHotData>> FetchHotCryptoDataAsync()
        {

            var hotCryptoDataList = new List<CryptoHotData>();
            var url = "https://www.okx.com/markets/explore/hot-crypto"; // 示例URL
            try
            {
                var response = await client.GetStringAsync(url);
                var document = new HtmlDocument();
                document.LoadHtml(response);

                var rows = document.DocumentNode.SelectNodes("//tr");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 7) continue;

                    // 获取排名
                    var rank = cells[0].SelectSingleNode(".//span").InnerText.Trim();

                    // 获取交易对
                    var symbol = cells[1].SelectSingleNode(".//a").InnerText.Trim();

                    // 获取当前价格
                    var price = cells[2].SelectSingleNode(".//div[contains(@class, 'index_last__1E-Bb')]").InnerText.Trim();

                    // 获取法币价格
                    var fiatPrice = cells[2].SelectSingleNode(".//div[contains(@class, 'index_fiat__5hPMQ')]").InnerText.Trim();

                    // 获取涨跌幅
                    var change = cells[3].SelectSingleNode(".//span[contains(@class, 'index_chg__Q4a4E')]").InnerText.Trim();

                    // 获取交易量
                    var volume = cells[4].InnerText.Trim();

                    // 获取24小时交易额
                    var tradingVolume24h = cells[5].InnerText.Trim();

                    hotCryptoDataList.Add(new CryptoHotData
                    {
                        Rank = rank,
                        Symbol = symbol,
                        Price = price,
                        FiatPrice = fiatPrice,
                        Change = change,
                        Volume = volume,
                        TradingVolume24h = tradingVolume24h
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                // 捕获HTTP请求异常并记录或跳过
                Console.WriteLine($"HTTP请求错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕获其他异常并记录或跳过
                Console.WriteLine($"未知错误: {ex.Message}");
            }


            return hotCryptoDataList;
        }


        public async Task<List<CryptoGainersLosersData>> FetchGainersLosersDataAsync()
        {
            var url = "https://www.okx.com/markets/explore/gainers-losers"; // 示例URL
            var gainersLosersData = new List<CryptoGainersLosersData>();
            try
            {
                var response = await client.GetStringAsync(url);
                var document = new HtmlDocument();
                document.LoadHtml(response);



                var rows = document.DocumentNode.SelectNodes("//tr");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 7) continue;

                    // 获取排名
                    var rank = cells[0].SelectSingleNode(".//span").InnerText.Trim();

                    // 获取交易对
                    var symbol = cells[1].SelectSingleNode(".//a").InnerText.Trim();

                    // 获取当前价格
                    var price = cells[2].SelectSingleNode(".//div[contains(@class, 'index_last__1E-Bb')]").InnerText.Trim();

                    // 获取法币价格
                    var fiatPrice = cells[2].SelectSingleNode(".//div[contains(@class, 'index_fiat__5hPMQ')]").InnerText.Trim();

                    // 获取涨跌幅
                    var change = cells[3].SelectSingleNode(".//span[contains(@class, 'index_chg__Q4a4E')]").InnerText.Trim();

                    // 获取交易量
                    var volume = cells[4].InnerText.Trim();

                    // 获取24小时交易额
                    var tradingVolume24h = cells[5].InnerText.Trim();

                    gainersLosersData.Add(new CryptoGainersLosersData
                    {
                        Rank = rank,
                        Symbol = symbol,
                        Price = price,
                        FiatPrice = fiatPrice,
                        Change = change,
                        Volume = volume,
                        TradingVolume24h = tradingVolume24h
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                // 捕获HTTP请求异常并记录或跳过
                Console.WriteLine($"HTTP请求错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕获其他异常并记录或跳过
                Console.WriteLine($"未知错误: {ex.Message}");
            }


            return gainersLosersData;
        }


        public async Task<List<CryptoLosersGainersData>> FetchLosersGainersDataAsync()
        {
            var url = "https://www.okx.com/markets/explore/losers-gainers"; // 示例URL
            var losersGainersData = new List<CryptoLosersGainersData>();
            try
            {
                var response = await client.GetStringAsync(url);
                var document = new HtmlDocument();
                document.LoadHtml(response);



                // 使用 XPath 选择所有包含市场数据的行
                var rows = document.DocumentNode.SelectNodes("//tr");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 7) continue; // 确保有足够的列

                    // 获取排名
                    var rank = cells[0].SelectSingleNode(".//span").InnerText.Trim();

                    // 获取交易对
                    var symbol = cells[1].SelectSingleNode(".//a").InnerText.Trim();

                    // 获取当前价格
                    var price = cells[2].SelectSingleNode(".//div[contains(@class, 'index_last__1E-Bb')]").InnerText.Trim();

                    // 获取法币价格
                    var fiatPrice = cells[2].SelectSingleNode(".//div[contains(@class, 'index_fiat__5hPMQ')]").InnerText.Trim();

                    // 获取变化百分比
                    var change = cells[3].SelectSingleNode(".//span[contains(@class, 'index_chg__Q4a4E')]").InnerText.Trim();

                    // 获取交易量
                    var volume = cells[4].InnerText.Trim();

                    // 获取24小时交易额
                    var tradingVolume24h = cells[5].InnerText.Trim();

                    // 添加到列表
                    losersGainersData.Add(new CryptoLosersGainersData
                    {
                        Rank = rank,
                        Symbol = symbol,
                        Price = price,
                        FiatPrice = fiatPrice,
                        Change = change,
                        Volume = volume,
                        TradingVolume24h = tradingVolume24h
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                // 捕获HTTP请求异常并记录或跳过
                Console.WriteLine($"HTTP请求错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕获其他异常并记录或跳过
                Console.WriteLine($"未知错误: {ex.Message}");
            }


            return losersGainersData;
        }


        public async Task<List<CryptoNewData>> FetchNewCryptoDataAsync()
        {
            var url = "https://www.okx.com/markets/explore/new-crypto"; // 示例URL
            var newCryptoDataList = new List<CryptoNewData>();
            try
            {
                var response = await client.GetStringAsync(url);
                var document = new HtmlDocument();
                document.LoadHtml(response);

                var rows = document.DocumentNode.SelectNodes("//tr");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 7) continue;

                    // 获取交易对
                    var symbol = cells[1].SelectSingleNode(".//a").InnerText.Trim();

                    // 获取发布日期或倒计时
                    string dateOrCountdown = cells[2].InnerText.Trim();

                    // 获取价格信息（如果有）
                    string price = "--", fiatPrice = "--";
                    if (cells[3].SelectSingleNode(".//div[contains(@class, 'index_last__1E-Bb')]") != null)
                    {
                        price = cells[3].SelectSingleNode(".//div[contains(@class, 'index_last__1E-Bb')]").InnerText.Trim();
                        fiatPrice = cells[3].SelectSingleNode(".//div[contains(@class, 'index_fiat__5hPMQ')]").InnerText.Trim();
                    }

                    // 获取涨跌幅
                    string change = cells[4].SelectSingleNode(".//span[contains(@class, 'index_chg__Q4a4E')]")?.InnerText.Trim() ?? "--";

                    // 获取交易量
                    string volume = cells[5].InnerText.Trim();

                    // 获取24小时交易额
                    string tradingVolume24h = cells[6].InnerText.Trim();

                    // 判断是否为即将发布的（含倒计时）
                    bool isUpcoming = dateOrCountdown.Contains("D") || dateOrCountdown.Contains("H") || dateOrCountdown.Contains("M");

                    newCryptoDataList.Add(new CryptoNewData
                    {
                        Symbol = symbol,
                        DateOrCountdown = dateOrCountdown,
                        IsUpcoming = isUpcoming,
                        Price = price,
                        FiatPrice = fiatPrice,
                        Change = change,
                        Volume = volume,
                        TradingVolume24h = tradingVolume24h
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                // 捕获HTTP请求异常并记录或跳过
                Console.WriteLine($"HTTP请求错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕获其他异常并记录或跳过
                Console.WriteLine($"未知错误: {ex.Message}");
            }


            return newCryptoDataList;
        }



        public async Task<List<TrendingCrypto>> FetchTrendingCryptoDataAsync()
        {
            var url = "https://www.okx.com/markets/explore/trending-crypto"; // 适当的 URL
            var trendingCryptos = new List<TrendingCrypto>();
            try
            {
                var response = await client.GetStringAsync(url);
                var document = new HtmlDocument();
                document.LoadHtml(response);



                // 使用 XPath 选择所有包含交易数据的行
                var rows = document.DocumentNode.SelectNodes("//tr");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 7) continue; // 确保有足够的列

                    // 获取排名
                    var rank = cells[0].SelectSingleNode(".//span").InnerText.Trim();

                    // 获取交易对
                    var symbol = cells[1].SelectSingleNode(".//a").InnerText.Trim();

                    // 获取当前价格
                    var price = cells[2].SelectSingleNode(".//div[contains(@class, 'index_last__1E-Bb')]").InnerText.Trim();

                    // 获取法币价格
                    var fiatPrice = cells[2].SelectSingleNode(".//div[contains(@class, 'index_fiat__5hPMQ')]").InnerText.Trim();

                    // 获取变化百分比
                    var change = cells[3].SelectSingleNode(".//span[contains(@class, 'index_chg__Q4a4E')]").InnerText.Trim();

                    // 获取交易量
                    var volume = cells[4].InnerText.Trim();

                    // 获取市值
                    var marketCap = cells[5].InnerText.Trim();

                    // 添加到列表
                    trendingCryptos.Add(new TrendingCrypto
                    {
                        Rank = rank,
                        Symbol = symbol,
                        Price = price,
                        FiatPrice = fiatPrice,
                        Change = change,
                        Volume = volume,
                        MarketCap = marketCap
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                // 捕获HTTP请求异常并记录或跳过
                Console.WriteLine($"HTTP请求错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕获其他异常并记录或跳过
                Console.WriteLine($"未知错误: {ex.Message}");
            }


            return trendingCryptos;
        }


        public async Task<List<CryptoMarketCap>> FetchCryptoMarketCapDataAsync()
        {
            var url = "https://www.okx.com/markets/explore/crypto-market-cap"; // 示例URL
            var marketCapData = new List<CryptoMarketCap>();

            try
            {
                var response = await client.GetStringAsync(url);
                var document = new HtmlDocument();
                document.LoadHtml(response);



                // 使用 XPath 选择所有包含市场数据的行
                var rows = document.DocumentNode.SelectNodes("//tr");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 8) continue; // 确保有足够的列

                    // 获取排名
                    var rank = cells[0].SelectSingleNode(".//span").InnerText.Trim();

                    // 获取交易对
                    var symbol = cells[1].SelectSingleNode(".//a").InnerText.Trim();

                    // 获取市值
                    var marketCap = cells[2].InnerText.Trim();

                    // 获取当前价格
                    var price = cells[3].SelectSingleNode(".//div[contains(@class, 'index_last__1E-Bb')]").InnerText.Trim();

                    // 获取法币价格
                    var fiatPrice = cells[3].SelectSingleNode(".//div[contains(@class, 'index_fiat__5hPMQ')]").InnerText.Trim();

                    // 获取变化百分比
                    var change = cells[4].SelectSingleNode(".//span[contains(@class, 'index_chg__Q4a4E')]").InnerText.Trim();

                    // 获取交易量
                    var volume = cells[5].InnerText.Trim();

                    // 获取24小时交易额
                    var tradingVolume24h = cells[6].InnerText.Trim();

                    // 添加到列表
                    marketCapData.Add(new CryptoMarketCap
                    {
                        Rank = rank,
                        Symbol = symbol,
                        MarketCap = marketCap,
                        Price = price,
                        FiatPrice = fiatPrice,
                        Change = change,
                        Volume = volume,
                        TradingVolume24h = tradingVolume24h
                    });
                }

            }
            catch (HttpRequestException ex)
            {
                // 捕获HTTP请求异常并记录或跳过
                Console.WriteLine($"HTTP请求错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕获其他异常并记录或跳过
                Console.WriteLine($"未知错误: {ex.Message}");
            }

            return marketCapData;
        }


        public async Task<List<CryptoVolumeData>> FetchCryptoVolumeDataAsync()
        {
            var url = "https://www.okx.com/markets/explore/crypto-volume"; // 示例URL

            var volumeData = new List<CryptoVolumeData>();

            try
            {
                var response = await client.GetStringAsync(url);
                var document = new HtmlDocument();
                document.LoadHtml(response);


                // 使用 XPath 选择所有包含市场数据的行
                var rows = document.DocumentNode.SelectNodes("//tr");

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 7) continue; // 确保有足够的列

                    // 获取排名
                    var rank = cells[0].SelectSingleNode(".//span").InnerText.Trim();

                    // 获取交易对
                    var symbol = cells[1].SelectSingleNode(".//a").InnerText.Trim();

                    // 获取24小时交易额
                    var tradingVolume24h = cells[2].InnerText.Trim();

                    // 获取当前价格
                    var price = cells[3].SelectSingleNode(".//div[contains(@class, 'index_last__1E-Bb')]").InnerText.Trim();

                    // 获取法币价格
                    var fiatPrice = cells[3].SelectSingleNode(".//div[contains(@class, 'index_fiat__5hPMQ')]").InnerText.Trim();

                    // 获取变化百分比
                    var change = cells[4].SelectSingleNode(".//span[contains(@class, 'index_chg__Q4a4E')]").InnerText.Trim();

                    // 获取交易量
                    var volume = cells[5].InnerText.Trim();

                    // 添加到列表
                    volumeData.Add(new CryptoVolumeData
                    {
                        Rank = rank,
                        Symbol = symbol,
                        TradingVolume24h = tradingVolume24h,
                        Price = price,
                        FiatPrice = fiatPrice,
                        Change = change,
                        Volume = volume
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                // 捕获HTTP请求异常并记录或跳过
                Console.WriteLine($"HTTP请求错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 捕获其他异常并记录或跳过
                Console.WriteLine($"未知错误: {ex.Message}");
            }


            return volumeData;
        }

    }

    public struct CryptoHotData
    {
        public string Rank { get; set; }
        public string Symbol { get; set; }
        public string Price { get; set; }
        public string FiatPrice { get; set; }
        public string Change { get; set; }
        public string Volume { get; set; }
        public string TradingVolume24h { get; set; }
    }


    public struct CryptoGainersLosersData
    {
        public string Rank { get; set; }
        public string Symbol { get; set; }
        public string Price { get; set; }
        public string FiatPrice { get; set; }
        public string Change { get; set; }
        public string Volume { get; set; }
        public string TradingVolume24h { get; set; }
    }


    public struct CryptoLosersGainersData
    {
        public string Rank { get; set; }
        public string Symbol { get; set; }
        public string Price { get; set; }
        public string FiatPrice { get; set; }
        public string Change { get; set; }
        public string Volume { get; set; }
        public string TradingVolume24h { get; set; }
    }


    public struct CryptoNewData
    {
        public string Symbol { get; set; }
        public string DateOrCountdown { get; set; }
        public bool IsUpcoming { get; set; }
        public string Price { get; set; }
        public string FiatPrice { get; set; }
        public string Change { get; set; }
        public string Volume { get; set; }
        public string TradingVolume24h { get; set; }
    }


    public struct TrendingCrypto
    {
        public string Rank { get; set; }
        public string Symbol { get; set; }
        public string Price { get; set; }
        public string FiatPrice { get; set; }
        public string Change { get; set; }
        public string Volume { get; set; }
        public string MarketCap { get; set; }
    }

    public struct CryptoMarketCap
    {
        public string Rank { get; set; }
        public string Symbol { get; set; }
        public string MarketCap { get; set; }
        public string Price { get; set; }
        public string FiatPrice { get; set; }
        public string Change { get; set; }
        public string Volume { get; set; }
        public string TradingVolume24h { get; set; }
    }


    public struct CryptoVolumeData
    {
        public string Rank { get; set; }
        public string Symbol { get; set; }
        public string TradingVolume24h { get; set; }
        public string Price { get; set; }
        public string FiatPrice { get; set; }
        public string Change { get; set; }
        public string Volume { get; set; }
    }


    // 其他结构体可以继续定义...
}
