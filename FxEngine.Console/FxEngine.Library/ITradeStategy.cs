using System;
using System.Collections.Generic;
using System.Text;

namespace FxEngine.Library
{
    public interface ITradeStategy
    {
        TradeDecision Evaluate(Candle candle);
    }


    public class TradeDecision
    {
        public decimal StopWinPrice { get; set; }

        public decimal StopLossPrice { get; set; }

        public TradeDecisionKind TradeDecisionKind { get; set; }

        public bool ShouldTrade { get; set; }
    }

    public enum TradeDecisionKind
    {
        Idle,
        Long,
        Short
    }


}
