using ccxt;
using ClosedXML.Excel;
using OkxTradingBot.UI.ViewModel;

namespace OkxTradingBot.Core.Utils
{
    public class ExcelExporter
    {
        public void ExportOrdersToExcel(List<Order> orders, string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Orders");

            worksheet.Cell(1, 1).Value = "Order ID";
            worksheet.Cell(1, 2).Value = "Symbol";
            worksheet.Cell(1, 3).Value = "Price";
            worksheet.Cell(1, 4).Value = "Amount";

            for (int i = 0; i < orders.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = orders[i].id;
                worksheet.Cell(i + 2, 2).Value = orders[i].symbol;
                worksheet.Cell(i + 2, 3).Value = orders[i].price;
                worksheet.Cell(i + 2, 4).Value = orders[i].amount;
            }

            workbook.SaveAs(filePath);
        }

        public void ExportTradeCycleToExcel(TradeCycle trade, string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Trade Cycles");

            // 设置表头
            worksheet.Cell(1, 1).Value = "Buy Price";
            worksheet.Cell(1, 2).Value = "Sell Price";
            worksheet.Cell(1, 3).Value = "Buy Time";
            worksheet.Cell(1, 4).Value = "Sell Time";
            worksheet.Cell(1, 5).Value = "Symbol";
            worksheet.Cell(1, 6).Value = "Score";

            // 设置 MACD 和 蜡烛图数据表头
            int currentColumn = 7;
            for (int i = 0; i < 5; i++)
            {
                worksheet.Cell(1, currentColumn++).Value = $"Buy MACD {i + 1}";
                worksheet.Cell(1, currentColumn++).Value = $"Sell MACD {i + 1}";
                worksheet.Cell(1, currentColumn++).Value = $"Buy Candle {i + 1}";
                worksheet.Cell(1, currentColumn++).Value = $"Sell Candle {i + 1}";
            }

            // 填充交易数据
            int row = 2;
            currentColumn = 1;
            worksheet.Cell(row, currentColumn++).Value = trade.BuyPrice;
            worksheet.Cell(row, currentColumn++).Value = trade.SellPrice;
            worksheet.Cell(row, currentColumn++).Value = trade.BuyTime;
            worksheet.Cell(row, currentColumn++).Value = trade.SellTime;
            worksheet.Cell(row, currentColumn++).Value = trade.Symbol;
            worksheet.Cell(row, currentColumn++).Value = trade.Score;

            // 填充 MACD 和 蜡烛图数据
            for (int i = 0; i < 5; i++)
            {
                worksheet.Cell(row, currentColumn++).Value = i < trade.BuyMacdValues.Count ? trade.BuyMacdValues[i] : 0;
                worksheet.Cell(row, currentColumn++).Value = i < trade.SellMacdValues.Count ? trade.SellMacdValues[i] : 0;
                worksheet.Cell(row, currentColumn++).Value = i < trade.BuyCandlestickValues.Count ? trade.BuyCandlestickValues[i] : 0;
                worksheet.Cell(row, currentColumn++).Value = i < trade.SellCandlestickValues.Count ? trade.SellCandlestickValues[i] : 0;
            }

            workbook.SaveAs(filePath);
        }
    }
}
