using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTec.TA.Library;

namespace FxEngine.Library
{
    public class StochasticStats : AbstractCandleStat, IFeaturable
    {

        private readonly Dictionary<DateTime, Stochastic> _statistics;

        public DateTime Begin => _statistics.Keys.Min();

        public DateTime End => _statistics.Keys.Max();


        Period IFeaturable.Period => Period;

        int IFeaturable.PeriodCount => PeriodCount;

        public StochasticStats(CandleCollection collection) : base(collection, 20)
        {
            _statistics = new Dictionary<DateTime, Stochastic>();
        }

        protected override void ReceiveCandles(Candle[] candles, DateTime dateTime)
        {
            int beginIdx = 0;
            int count = candles.Length;
            double[] k = new double[candles.Length];
            double[] d = new double[candles.Length];

            Core.Stoch(0, candles.Length-1,
                                candles.Select(c => (float)c.High).ToArray(),
                                candles.Select(c => (float)c.Low).ToArray(),
                                candles.Select(c => (float)c.Close).ToArray(),
                                5, 3, Core.MAType.Sma, 3, Core.MAType.Sma, out beginIdx, out count, k, d);

            _statistics[dateTime] = new Stochastic()
            {
                K = k[0],
                D = d[0]
            };

            if(_statistics.Count() >= candles.Length)
            {
                for(int i=0; i< candles.Length; i++)
                {
                    dateTime = dateTime.AddPeriod(Period, PeriodCount, -1);
                    if (!_statistics.ContainsKey(dateTime))
                    {
                        _statistics[dateTime] = new Stochastic()
                        {
                            K = k[i],
                            D = d[i]
                        };

                    }
                }
            }
        }

        public Stochastic GetValue(DateTime dateTime)
        {
            if (_statistics.ContainsKey(dateTime))
            {
                return _statistics[dateTime];
            }

            return null;
        }

        public virtual IEnumerable<string> GetHeaders()
        {
            return new string[] { $"{Period.ToString()}{PeriodCount}STOCHK",
                                   $"{Period.ToString()}{PeriodCount}STOCHD",

            };
        }

        public virtual IDictionary<string, float> GetFeatures(DateTime dateTime)
        {
            var result = new Dictionary<string, float>();
            var stat = _statistics[dateTime];

            result[$"{Period.ToString()}{PeriodCount}STOCHK"] = (float)(stat.K);
            result[$"{Period.ToString()}{PeriodCount}STOCHD"] = (float)(stat.D);

            return result;
        }

        public virtual bool HasFeatures(DateTime dateTime)
        {
            return _statistics.ContainsKey(dateTime);
        }
    }


    public class Stochastic
    {
        public double D { get; set; }

        public double K { get; set; }

        public override string ToString()
        {
            return $"({D};{K})";
        }
    }
}
