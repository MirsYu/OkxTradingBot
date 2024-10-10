using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OkxTradingBot.Core.Strategies
{
    // 定义缠论中的分型类型：顶分型、底分型、无分型
    public enum FractalType { Top, Bottom, None }
    public enum Trend { Up, Down, None }
    public class ChanLunStrategy
    {
        // 检测分型的方法，根据蜡烛图数据
        public FractalType DetectFractal(List<Candle> candles, int index)
        {
            if (index < 1 || index >= candles.Count - 1)
                return FractalType.None;

            var prev = candles[index - 1];
            var curr = candles[index];
            var next = candles[index + 1];

            // 顶分型条件
            if (prev.High < curr.High && curr.High > next.High)
                return FractalType.Top;

            // 底分型条件
            if (prev.Low > curr.Low && curr.Low < next.Low)
                return FractalType.Bottom;

            return FractalType.None;
        }

        // 判断当前趋势：上升趋势、下降趋势或无趋势
        public Trend DetermineTrend(List<Candle> candles)
        {
            Trend currentTrend = Trend.None;
            for (int i = 1; i < candles.Count - 1; i++)
            {
                var fractal = DetectFractal(candles, i);

                if (fractal == FractalType.Bottom)
                {
                    currentTrend = Trend.Up; // 出现底分型，进入上升趋势
                }
                else if (fractal == FractalType.Top)
                {
                    currentTrend = Trend.Down; // 出现顶分型，进入下降趋势
                }
            }
            return currentTrend;
        }

        // 检测是否进入中枢区域
        public class PivotZone
        {
            public decimal UpperBound { get; set; }
            public decimal LowerBound { get; set; }

            public bool IsWithinPivot(decimal price)
            {
                return price >= LowerBound && price <= UpperBound;
            }
        }

        public PivotZone DetectPivot(List<Candle> candles)
        {
            var pivotZone = new PivotZone();
            pivotZone.UpperBound = candles.Take(3).Max(c => c.High);
            pivotZone.LowerBound = candles.Take(3).Min(c => c.Low);
            return pivotZone;
        }

        // 检测背驰，根据MACD或其他指标判断背驰
        public bool CheckForDivergence(List<Candle> candles, List<decimal> macdValues, int index)
        {
            if (index < 1 || index >= macdValues.Count - 1)
                return false;

            // 顶背驰：价格创新高，但MACD没有创新高
            if (candles[index].High > candles[index - 1].High && macdValues[index] < macdValues[index - 1])
                return true;

            // 底背驰：价格创新低，但MACD没有创新低
            if (candles[index].Low < candles[index - 1].Low && macdValues[index] > macdValues[index - 1])
                return true;

            return false;
        }

        // 生成交易信号，基于趋势、分型和背驰判断买卖
        public string GenerateSignal(List<Candle> candles, List<decimal> macdValues)
        {
            for (int i = 1; i < candles.Count - 1; i++)
            {
                var fractal = DetectFractal(candles, i);
                var trend = DetermineTrend(candles);

                if (trend == Trend.Up && fractal == FractalType.Top && CheckForDivergence(candles, macdValues, i))
                {
                    return "Sell"; // 在上升趋势中出现顶背驰，生成卖出信号
                }

                if (trend == Trend.Down && fractal == FractalType.Bottom && CheckForDivergence(candles, macdValues, i))
                {
                    return "Buy"; // 在下降趋势中出现底背驰，生成买入信号
                }
            }
            return "Hold"; // 无操作信号
        }
    }

    public class Candle
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public DateTime Time { get; set; }
    }
}
