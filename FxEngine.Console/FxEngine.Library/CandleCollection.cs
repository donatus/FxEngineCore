using System;
using System.Collections.Generic;
using System.Text;

namespace FxEngine.Library
{
    public class CandleCollection
    {
        public string Instrument { get; private set; }

        public Period Period { get; private set; }

        public int PeriodCount { get; private set; }

        private readonly Dictionary<DateTime, Candle> _list;

        public delegate void CandleAddedHandler(object sender, Candle addedCandle);

        public event CandleAddedHandler CandleAddedEvent;

        public CandleCollection(string instrument, Period period, int periodcount)
        {
            Instrument = instrument;
            Period = period;
            PeriodCount = periodcount;
            _list = new Dictionary<DateTime, Candle>();
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

            _list[dateTime] = candle;

            CandleAddedEvent?.Invoke(this, candle);
        }


    }
}
