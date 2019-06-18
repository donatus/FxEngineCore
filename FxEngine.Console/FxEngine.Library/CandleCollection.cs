using System;
using System.Collections.Generic;
using System.Text;

namespace FxEngine.Library
{
    public class CandleCollection : IFeaturable
    {
        public string Instrument { get; private set; }

        public Period Period { get; private set; }

        public int PeriodCount { get; private set; }

        public readonly Dictionary<DateTime, Candle> _list;

        public delegate void CandleAddedHandler(object sender, Candle addedCandle);

        public event CandleAddedHandler CandleAddedEvent;

        public CandleCollection(string instrument, Period period, int periodcount)
        {
            Instrument = instrument;
            Period = period;
            PeriodCount = periodcount;
            _list = new Dictionary<DateTime, Candle>();
        }

        protected void AddCandle(Candle consolidated)
        {
            _list[consolidated.DateTime] = consolidated;

            //verify if previous candles are setted
            if (_list.Count > 1)
            {
                FeedPrevious(consolidated.DateTime);
            }

            CandleAddedEvent?.Invoke(this, consolidated);
        }

        public void AddCandle(DateTime dateTime,
                                    decimal open,
                                    decimal close,
                                    decimal high,
                                    decimal low,
                                    int volume)
        {
            

            Candle candle = new Candle()
            {
                DateTime = dateTime,
                Open = open,
                Close = close,
                High = high,
                Low = low,
                Instrument = Instrument,
                Period = Period,
                PeriodCount = PeriodCount,
                Volume = volume
            };

            AddCandle(candle);
            
        }

        public Candle GetCandle(DateTime date)
        {
            if (_list.ContainsKey(date))
            {
                return _list[date];
            }

            return null;
        }

        private void FeedPrevious(DateTime dateTime)
        {
            DateTime previous = dateTime.AddPeriod(Period, PeriodCount, -1);

            if (_list.ContainsKey(previous))
            {
                return;
            }

            while (!_list.ContainsKey(previous))
            {
                previous = previous.AddPeriod(Period, PeriodCount, -1);
            }

            Candle lastCandle = _list[previous];

            foreach (DateTime date in previous.GetPeriods(dateTime, Period, PeriodCount))
            {
                if (!_list.ContainsKey(date))
                {
                    _list[date] = Candle.CreateZeroCandle(lastCandle, date);
                }
            }
        }

        internal IEnumerable<Candle> GetCandles(DateTime from, DateTime to)
        {
            List<Candle> result = new List<Candle>();
            foreach(DateTime date in from.GetPeriods(to, Period, PeriodCount))
            {
                if (_list.ContainsKey(date))
                {
                    result.Add(_list[date]);
                }
            }

            return result;
        }

        public IEnumerable<string> GetHeaders()
        {
            return new string[] { $"{Period.ToString()}{PeriodCount}V",
                                   $"{Period.ToString()}{PeriodCount}C",
                                   $"{Period.ToString()}{PeriodCount}H",
                                   $"{Period.ToString()}{PeriodCount}L",

            };
        }

        public IEnumerable<float> GetFeatures(DateTime dateTime)
        {
            Candle candle = _list[dateTime];

            float[] result = new float[]
            {
                candle.Volume,
                decimal.ToSingle(candle.CloseRelativ),
                decimal.ToSingle(candle.HighRaltiv),
                decimal.ToSingle(candle.LowRelativ)
            };

            return result;
        }

        public bool HasFeatures(DateTime dateTime)
        {
            return _list.ContainsKey(dateTime);
        }
    }
}
