using System;
using System.Collections.Generic;
using System.Text;

namespace FxEngine.Library
{
    public class PrevisionTradeStrategy : ITradeStategy
    {
        private CandleCollection collection;
        private PipExpectation winpip;

        public PrevisionTradeStrategy(CandleCollection collection, PipExpectation winpip)
        {
            this.collection = collection;
            this.winpip = winpip;
        }

        public TradeDecision Evaluate(Candle candle)
        {
            var pip = winpip.GetValue(candle.DateTime);
            if (pip == null)
            {
                return new TradeDecision()
                {
                    ShouldTrade = false,
                    TradeDecisionKind = TradeDecisionKind.Idle
                };
            }

            if(pip.Long > 5m && pip.Short < 1.6m)
            {
                return new TradeDecision()
                {
                    ShouldTrade = true,
                    TradeDecisionKind = TradeDecisionKind.Long,
                    StopLossPrice = 2.0m,
                    StopWinPrice = 5.0m
                };
            }
            else if(pip.Short > 5m && pip.Long < 1.6m)
            {
                return new TradeDecision()
                {
                    ShouldTrade = true,
                    TradeDecisionKind = TradeDecisionKind.Short,
                    StopLossPrice = 2.0m,
                    StopWinPrice = 5.0m
                };
            }

            return new TradeDecision()
            {
                ShouldTrade = false,
                TradeDecisionKind = TradeDecisionKind.Idle
            };
        }
    }
}
