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

        public Period Period { get; set; }

        public int PeriodCount { get; set; }
    }

    public enum Period
    {
        Hour,
        Minute,
        Second
    }
}
