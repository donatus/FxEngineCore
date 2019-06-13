using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxEngine.Library
{
    public abstract class AbstractCandleStat
    {
        private readonly CandleCollection _collection;

        protected readonly Period Period;

        protected readonly int PeriodCount;

        protected readonly int PeriodWarmup;

        private readonly bool isFromConsolidatedCollection;

        protected AbstractCandleStat(CandleCollection collection, int periodWarmup)
        {
            PeriodWarmup = periodWarmup;
            _collection = collection;
            isFromConsolidatedCollection = collection is ConsolidatedCandleCollection;
            Period = isFromConsolidatedCollection ? (collection as ConsolidatedCandleCollection).ConsolidatedPeriod : collection.Period;
            PeriodCount = isFromConsolidatedCollection ? (collection as ConsolidatedCandleCollection).ConsolidatedPeriodCount : collection.PeriodCount;
            _collection.CandleAddedEvent += CandleAdded;
        }

        private void CandleAdded(object sender, Candle addedCandle)
        {
            DateTime beginPeriod = addedCandle.DateTime.AddPeriod(Period, PeriodCount, -PeriodWarmup);
            Candle[] candles;

            if (isFromConsolidatedCollection)
            {
                candles = (_collection as ConsolidatedCandleCollection).GetCandlesWithConsolidatedPeriod(beginPeriod, addedCandle.DateTime).ToArray();
            }
            else
            {

                candles = _collection.GetCandles(beginPeriod, addedCandle.DateTime).ToArray();

                
            }
            if (candles.Length > PeriodWarmup)
            {
                ReceiveCandles(candles.OrderBy(c => c.DateTime).Reverse().ToArray(), addedCandle.DateTime);
            }
        }

        protected abstract void ReceiveCandles(Candle[] candles, DateTime dateTime);

    }
}
