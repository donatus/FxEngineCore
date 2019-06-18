using FxEngine.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxEngine.ML
{
    public class TradeSimulation
    {
        private readonly CandleCollection _candleCollection;

        private readonly ITradeStategy _tradeStrategy;

        private List<TradePosition> _activity;

        public TradeSimulation(CandleCollection candleCollection, ITradeStategy tradeStrategy)
        {
            _candleCollection = candleCollection;
            _tradeStrategy = tradeStrategy;
            _activity = new List<TradePosition>();
        }

        public void Simulate(DateTime from, DateTime to)
        {
            DateTime dateTime = from;

            TradePosition tradePosition = null;

            while (dateTime < to)
            {
                Candle candle = _candleCollection.GetCandle(dateTime);
                if (candle == null)
                {
                    dateTime = dateTime.AddSeconds(5);
                    continue;
                }
                    
                if(tradePosition != null)
                {
                    tradePosition.Evaluate(candle.Close);

                    if (tradePosition.IsClosed)
                    {
                        _activity.Add(tradePosition);
                        tradePosition = null;
                    }
                }
                else
                {
                    TradeDecision decision = _tradeStrategy.Evaluate(candle);
                    if (decision.ShouldTrade)
                    {
                        tradePosition = new TradePosition(dateTime, candle.Close, decision);
                    }
                }

                dateTime = dateTime.AddSeconds(5);
            }

            Console.WriteLine($"Win {_activity.Sum(c => c.Win)} Loss {_activity.Sum(c => c.Loss)}");
        }
    }

    internal class TradePosition
    {
        private DateTime dateTime;
        private decimal open;
        private decimal Close;
        private TradeDecision decision;

        private decimal StopLossPrice;

        private decimal StopWinPrice;

        public decimal Win;

        public decimal Loss;

        public TradePosition(DateTime dateTime, decimal close, TradeDecision decision)
        {
            this.dateTime = dateTime;
            this.open = close;
            this.decision = decision;
            IsClosed = false;

            switch (decision.TradeDecisionKind)
            {
                case TradeDecisionKind.Long:
                    open = open - 0.00015m; //spread
                    StopWinPrice = open + (0.0001m * decision.StopWinPrice);
                    StopLossPrice = open - (0.0001m * decision.StopLossPrice);
                    
                    break;
                case TradeDecisionKind.Short:
                    open = open + 0.00015m; //spread
                    StopWinPrice = open - (0.0001m * decision.StopWinPrice);
                    StopLossPrice = open + (0.0001m * decision.StopLossPrice);
                    break;
            }
        }

        public bool IsClosed { get; internal set; }

        internal void Evaluate(decimal close)
        {
            switch (decision.TradeDecisionKind)
            {
                case TradeDecisionKind.Long:
                    if(close >= StopWinPrice)
                    {
                        IsClosed = true;
                        Close = close;
                        Win = (close - open) * 10000;
                    }
                    else if(close <= StopLossPrice)
                    {
                        IsClosed = true;
                        Close = close;
                        Loss = (close - open) * 10000;
                    }
                    break;
                case TradeDecisionKind.Short:
                    if (close <= StopWinPrice)
                    {
                        IsClosed = true;
                        Close = close;
                        Win = (open - close) * 10000;

                    }
                    else if (close >= StopLossPrice)
                    {
                        IsClosed = true;
                        Close = close;
                        Loss = (open - close) * 10000;
                    }
                    break;
            }
        }
    }
}
