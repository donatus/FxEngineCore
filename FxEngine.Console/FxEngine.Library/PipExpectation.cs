using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FxEngine.Library
{
    public class PipExpectation
    {

        public readonly Dictionary<DateTime, PipWin> _futurPip;

        private readonly int _periodCount;

        private readonly CandleCollection _collection;

        public PipExpectation(CandleCollection collection, int periodCount)
        {
            _periodCount = periodCount;
            _futurPip = new Dictionary<DateTime, PipWin>();
            _collection = collection;
            collection.CandleAddedEvent += AddedCandle;
        }

        private void AddedCandle(object sender, Candle addedCandle)
        {
            DateTime eventTime = addedCandle.DateTime;
            DateTime startTime = eventTime.AddPeriod(_collection.Period, _collection.PeriodCount, -_periodCount);

            var candles =_collection.GetCandles(startTime, eventTime);

            if(candles.Count() < _periodCount)
            {
                return;
            }

            Candle startCandle = candles.First();

            decimal longPip = candles.Max(c => c.Close) - startCandle.Open;
            decimal shortPip = startCandle.Open - candles.Min(c => c.Close);

            longPip = longPip < 0 ? 0 : longPip * 10000;
            shortPip = shortPip < 0 ? 0 : shortPip * 10000;

            _futurPip[startTime] = new PipWin()
            {
                Short = shortPip,
                Long = longPip
            };


        }

        public PipWin GetValue(DateTime dateTime)
        {
            if (_futurPip.ContainsKey(dateTime))
            {
                return _futurPip[dateTime];
            }

            return null;
        }
    }

    public class PipWin
    {
        public decimal Short { get; set; }

        public decimal Long { get; set; }

        public override string ToString()
        {
            return $"L:{Long}-S:{Short}";
        }
    }
}
