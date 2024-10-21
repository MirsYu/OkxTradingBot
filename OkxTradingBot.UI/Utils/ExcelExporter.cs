using ccxt;
using ClosedXML.Excel;
using OkxTradingBot.UI.ViewModel;
using System.IO;

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
            // Load existing workbook or create a new one
            using var workbook = new FileInfo(filePath).Exists ? new XLWorkbook(filePath) : new XLWorkbook();
            var worksheet = workbook.Worksheets.FirstOrDefault() ?? workbook.Worksheets.Add("Trade Cycles");

            // Find the last row with data
            int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            // Declare currentColumn outside of the if block
            int currentColumn;

            // Set headers if this is a new sheet
            if (lastRow == 1)
            {
                worksheet.Cell(1, 1).Value = "Buy Price";
                worksheet.Cell(1, 2).Value = "Sell Price";
                worksheet.Cell(1, 3).Value = "Buy Time";
                worksheet.Cell(1, 4).Value = "Sell Time";
                worksheet.Cell(1, 5).Value = "Symbol";
                worksheet.Cell(1, 6).Value = "Score";

                currentColumn = 7; // Initialize currentColumn here
                for (int i = 0; i < 5; i++)
                {
                    worksheet.Cell(1, currentColumn++).Value = $"Buy MACD Line {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Sell MACD Line {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Buy Candle Open {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Buy Candle High {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Buy Candle Low {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Buy Candle Close {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Sell Candle Open {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Sell Candle High {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Sell Candle Low {i + 1}";
                    worksheet.Cell(1, currentColumn++).Value = $"Sell Candle Close {i + 1}";
                }
            }
            else
            {
                currentColumn = 1; // Initialize for appending data
            }

            // Fill trade data in the next row
            int row = lastRow + 1;
            currentColumn = 1; // Reset for the new row
            worksheet.Cell(row, currentColumn++).Value = trade.BuyPrice;
            worksheet.Cell(row, currentColumn++).Value = trade.SellPrice;
            worksheet.Cell(row, currentColumn++).Value = trade.BuyTime;
            worksheet.Cell(row, currentColumn++).Value = trade.SellTime;
            worksheet.Cell(row, currentColumn++).Value = trade.Symbol;
            worksheet.Cell(row, currentColumn++).Value = trade.Score;

            // Fill MACD and candlestick data
            for (int i = 0; i < 5; i++)
            {
                // Write MACD values
                if (i < trade.BuyMacdValues.Count)
                {
                    worksheet.Cell(row, currentColumn++).Value = trade.BuyMacdValues[i].MACDLine;
                }
                else
                {
                    worksheet.Cell(row, currentColumn++).Value = 0; // Default value if not available
                }

                if (i < trade.SellMacdValues.Count)
                {
                    worksheet.Cell(row, currentColumn++).Value = trade.SellMacdValues[i].MACDLine;
                }
                else
                {
                    worksheet.Cell(row, currentColumn++).Value = 0; // Default value if not available
                }

                // Write Buy Candle data
                if (i < trade.BuyCandlestickValues.Count)
                {
                    var buyCandle = trade.BuyCandlestickValues[i];
                    worksheet.Cell(row, currentColumn++).Value = buyCandle.Open;
                    worksheet.Cell(row, currentColumn++).Value = buyCandle.High;
                    worksheet.Cell(row, currentColumn++).Value = buyCandle.Low;
                    worksheet.Cell(row, currentColumn++).Value = buyCandle.Close;
                }
                else
                {
                    // Default values for Buy Candle if not available
                    worksheet.Cell(row, currentColumn++).Value = 0;
                    worksheet.Cell(row, currentColumn++).Value = 0;
                    worksheet.Cell(row, currentColumn++).Value = 0;
                    worksheet.Cell(row, currentColumn++).Value = 0;
                }

                // Write Sell Candle data
                if (i < trade.SellCandlestickValues.Count)
                {
                    var sellCandle = trade.SellCandlestickValues[i];
                    worksheet.Cell(row, currentColumn++).Value = sellCandle.Open;
                    worksheet.Cell(row, currentColumn++).Value = sellCandle.High;
                    worksheet.Cell(row, currentColumn++).Value = sellCandle.Low;
                    worksheet.Cell(row, currentColumn++).Value = sellCandle.Close;
                }
                else
                {
                    // Default values for Sell Candle if not available
                    worksheet.Cell(row, currentColumn++).Value = 0;
                    worksheet.Cell(row, currentColumn++).Value = 0;
                    worksheet.Cell(row, currentColumn++).Value = 0;
                    worksheet.Cell(row, currentColumn++).Value = 0;
                }
            }

            workbook.SaveAs(filePath);
        }



    }
}
