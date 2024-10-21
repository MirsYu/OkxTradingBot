using OkxTradingBot.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OkxTradingBot.Core.Strategies
{
    public enum FractalType { Top, Bottom, None }
    public enum Trend { Up, Down, None }

    public class ChanLunStrategy
    {
        // 检测分型的方法，考虑5根K线以增加分型的有效性
        //public FractalType DetectFractal(List<Candle> candles)
        //{
        //    if (candles.Count < 5)
        //        return FractalType.None;

        //    var prev3 = candles[candles.Count - 5];
        //    var prev2 = candles[candles.Count - 4];
        //    var prev1 = candles[candles.Count - 3];
        //    var curr = candles[candles.Count - 2];
        //    var next = candles[candles.Count - 1];

        //    // 顶分型条件：当前蜡烛中间三根高点依次上升，且后面一根的高点开始下降
        //    if (prev3.High < prev2.High && prev2.High < prev1.High && prev1.High > curr.High && curr.High > next.High)
        //        return FractalType.Top;

        //    // 底分型条件：当前蜡烛中间三根低点依次下降，且后面一根的低点开始上升
        //    if (prev3.Low > prev2.Low && prev2.Low > prev1.Low && prev1.Low < curr.Low && curr.Low < next.Low)
        //        return FractalType.Bottom;

        //    return FractalType.None;
        //}

        public FractalType DetectFractal(List<Candle> candles)
        {
            if (candles.Count < 5)
                return FractalType.None;

            var prev2 = candles[candles.Count - 3];
            var prev1 = candles[candles.Count - 2];
            var curr = candles[candles.Count - 1];

            // 顶分型条件：当前蜡烛高点大于前两根的高点
            if (prev2.High < prev1.High && prev1.High < curr.High)
                return FractalType.Top;

            // 底分型条件：当前蜡烛低点低于前两根的低点
            if (prev2.Low > prev1.Low && prev1.Low > curr.Low)
                return FractalType.Bottom;

            return FractalType.None;
        }

        // 中枢检测逻辑，基于最近 5 至 10 根蜡烛的高低点
        public class PivotZone
        {
            public decimal UpperBound { get; set; }
            public decimal LowerBound { get; set; }

            // 判断价格是否在中枢区域内
            public bool IsWithinPivot(decimal price)
            {
                return price >= LowerBound && price <= UpperBound;
            }
        }

        public PivotZone DetectPivot(List<Candle> candles, int period = 10)
        {
            var pivotZone = new PivotZone();
            pivotZone.UpperBound = candles.Take(period).Max(c => c.High);
            pivotZone.LowerBound = candles.Take(period).Min(c => c.Low);
            return pivotZone;
        }

        // 判断趋势方向
        public Trend DetermineTrend(List<Candle> candles)
        {
            // 简单以价格的高低点变化来判断趋势
            var latestCandle = candles.Last();
            var previousCandle = candles[candles.Count - 2];

            if (latestCandle.Close > previousCandle.Close)
                return Trend.Up;
            if (latestCandle.Close < previousCandle.Close)
                return Trend.Down;

            return Trend.None;
        }

        // 背驰检测，可以引入多个指标，如MACD、RSI
        public bool CheckForDivergence(List<Candle> candles, List<decimal> macdValues)
        {
            if (candles.Count < 2 || macdValues.Count < 2)
                return false;

            var latestCandle = candles.Last();
            var previousCandle = candles[candles.Count - 2];

            // 顶背驰：价格创新高，但MACD未创新高
            if (latestCandle.High > previousCandle.High && macdValues.Last() < macdValues[macdValues.Count - 2])
                return true;

            // 底背驰：价格创新低，但MACD未创新低
            if (latestCandle.Low < previousCandle.Low && macdValues.Last() > macdValues[macdValues.Count - 2])
                return true;

            return false;
        }

        // 基于缠论的多信号生成交易信号
        public string GenerateSignal(List<Candle> candles, List<decimal> macdValues)
        {
            // 获取当前趋势
            var trend = DetermineTrend(candles);

            // 检测当前分型
            var fractal = DetectFractal(candles);

            // 检测背驰
            bool divergence = CheckForDivergence(candles, macdValues);

            // 判断中枢区域
            var pivot = DetectPivot(candles);
            bool inPivotZone = pivot.IsWithinPivot(candles.Last().Close);

            // 交易逻辑
            if (trend == Trend.Up && fractal == FractalType.Top && divergence && !inPivotZone)
                return "Sell"; // 卖出信号

            if (trend == Trend.Down && fractal == FractalType.Bottom && divergence && !inPivotZone)
                return "Buy"; // 买入信号

            return "Hold"; // 无操作
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
