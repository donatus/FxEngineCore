using System;
using System.Collections.Generic;
using System.Text;

namespace FxEngine.Library
{
    public class Candle
    {
        public string Instrument { get; set; }

        public DateTime DateTime { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public int Volume { get; set; }

        public bool IsComplete { get; set; }

        public bool IsZero { get; set; }

        public Period Period { get; set; }

        public int PeriodCount { get; set; }

        public bool IsConsolidated { get; set; }

        public decimal CloseRelativ => Close - Open;
        public decimal HighRaltiv => High - Open;
        public decimal LowRelativ => Low - Open;

        public override string ToString()
        {
            return DateTime.ToString("y.MM.dd HH:mm:ss") + " - " + Close;
        }
        internal static Candle CreateZeroCandle(Candle lastCandle, DateTime dateTime)
        {
            Candle result = new Candle()
            {
                Instrument = lastCandle.Instrument,
                Close = lastCandle.Close,
                Open = lastCandle.Close,
                DateTime = dateTime,
                High = lastCandle.Close,
                Low = lastCandle.Close,
                IsZero = true,
                Period = lastCandle.Period,
                PeriodCount = lastCandle.PeriodCount,
                Volume = 0
            };
            return result;
        }
    }

    public enum Period
    {
        H = 3600,
        M = 60,
        S = 1
    }
}
