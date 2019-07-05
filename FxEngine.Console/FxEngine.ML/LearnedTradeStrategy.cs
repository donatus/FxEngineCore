using System;
using System.Collections.Generic;
using System.Text;
using FxEngine.Library;
using Microsoft.ML;
using System.Linq;

namespace FxEngine.ML
{
    public class LearnedTradeStrategy : ITradeStategy
    {
        private ITransformer _model;
        private FeaturesFactory _features;


        public LearnedTradeStrategy(FeaturesFactory features, ITransformer model)
        {
            _features = features;
            _model = model;
        }


        public void Initialize()
        {
            _features.InitializeEvaluation(_model);
        }

        public TradeDecision Evaluate(Candle candle)
        {
            //_features.AddData(candle);
            float[] scores = _features.Evaluate(_model, candle.DateTime);

            if(scores?.Average() > 5.0f)
            {
                return new TradeDecision()
                {
                    ShouldTrade = true,
                    StopLossPrice = 2,
                    StopWinPrice = 5,
                    TradeDecisionKind = TradeDecisionKind.Long
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
