using System.Collections.Generic;
using System.Linq;

namespace OkxTradingBot.Core.Strategies
{
    /// <summary>
    /// 实现交易策略
    /// </summary>
    public class MovingAverageStrategy
    {
        private readonly List<decimal> _prices = new List<decimal>();
        private readonly int _period;

        public MovingAverageStrategy(int period)
        {
            _period = period;
        }

        public void AddPrice(decimal price)
        {
            _prices.Add(price);
            if (_prices.Count > _period)
                _prices.RemoveAt(0);
        }

        public bool ShouldBuy()
        {
            if (_prices.Count < _period) return false;

            decimal avgPrice = _prices.Average();
            return _prices.Last() > avgPrice; // 如果当前价格高于移动平均线，则买入
        }

    }
}
