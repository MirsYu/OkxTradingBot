using ccxt;
using ClosedXML.Excel;

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
    }
}
