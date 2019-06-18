using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FxEngine.Library
{
    public class ConsolidatedCandleCollection : CandleCollection
    {
        private readonly CandleCollection _collectiontoconsolidate;

        private readonly Period _period;

        private readonly int _periodCount;

        private readonly int _consolidationCount;

        public ConsolidatedCandleCollection(CandleCollection candleCollection, Period period, int periodCount) : base(candleCollection.Instrument, candleCollection.Period, candleCollection.PeriodCount)
        {
            _collectiontoconsolidate = candleCollection;
            _period = period;
            _periodCount = periodCount;
            _collectiontoconsolidate.CandleAddedEvent += CandleAdded;

            int seconds = (int)period * periodCount;

            int periodseconds = (int)candleCollection.Period * candleCollection.PeriodCount;

            _consolidationCount = seconds / periodseconds;
        }

        public Period ConsolidatedPeriod => _period;

        public int ConsolidatedPeriodCount => _periodCount;


        private void CandleAdded(object sender, Candle addedCandle)
        {
            DateTime beginPeriod = addedCandle.DateTime.GetBeginPeriod(_period, _periodCount);

            IEnumerable<Candle> candles = _collectiontoconsolidate.GetCandles(beginPeriod, addedCandle.DateTime).ToList();

            if (candles.Count() <= _consolidationCount)
            {
                return;
            }

            Candle consolidated = new Candle()
            {
                Instrument = Instrument,
                Period = _period,
                PeriodCount = _periodCount,
                Close = addedCandle.Close,
                Open = candles.First().Open,
                DateTime = addedCandle.DateTime,
                Volume = candles.Sum(c => c.Volume),
                High = candles.Max(c => c.High),
                Low = candles.Min(c => c.Low),
                IsConsolidated = true
            };

            AddCandle(consolidated);
        }

        internal IEnumerable<Candle> GetCandlesWithConsolidatedPeriod(DateTime from, DateTime to)
        {
            List<Candle> result = new List<Candle>();
            foreach (DateTime date in from.GetPeriods(to, _period, _periodCount))
            {
                Candle candle = GetCandle(date);
                if (candle != null)
                {
                    result.Add(candle);
                }
            }

            return result;
        }

    }
}
