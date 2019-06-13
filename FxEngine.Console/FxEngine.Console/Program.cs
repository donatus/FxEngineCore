using CsvHelper;
using FxEngine.Library;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace FxEngine.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var candles = Oanda.LoadCandles("EUR_USD", "S5", DateTime.Now.AddDays(-30));

            var collection = new CandleCollection("EUR_USD", Period.Second, 5);
            var minuteconsolidation = new ConsolidatedCandleCollection(collection, Period.Minute, 1);
            var m15Consolidation = new ConsolidatedCandleCollection(collection, Period.Minute, 15);
            var stochasticS5 = new StochasticStats(collection);
            var stochasticM1 = new StochasticStats(minuteconsolidation);
            var stochasticM15 = new StochasticStats(m15Consolidation);
            var winpip = new PipExpectation(collection, 30);
            foreach (OandaCandle candle in candles)
            {
                DateTime datetime = new DateTime(candle.Time.Year, candle.Time.Month, candle.Time.Day, candle.Time.Hour, candle.Time.Minute, candle.Time.Second);
                collection.AddCandle(datetime, candle.Mid.O, candle.Mid.C, candle.Mid.H, candle.Mid.L, candle.Volume);
            }

            var maxlong = winpip._futurPip.Values.Max(c => c.Long);
            var avglong = winpip._futurPip.Values.Average(c => c.Long);
            int moreThanSpreadLong = winpip._futurPip.Values.Count(c => c.Long > 5m && c.Short < 1.5m);

            var maxshort = winpip._futurPip.Values.Max(c => c.Short);
            var avgshort = winpip._futurPip.Values.Average(c => c.Short);
            int moreThanSpreadShort = winpip._futurPip.Values.Count(c => c.Short > 5m && c.Long < 1.5m);


            var records = new List<dynamic>();
            foreach (DateTime dateTime in collection._list.Keys.OrderBy(c => c.Ticks))
            {
                var S5Candle = collection.GetCandle(dateTime);
                if (S5Candle == null) continue;

                var M1Candle = minuteconsolidation.GetCandle(dateTime);
                if (M1Candle == null) continue;

                var M15Candle = m15Consolidation.GetCandle(dateTime);
                if (M15Candle == null) continue;

                var S5Stoch = stochasticS5.GetValue(dateTime);
                if (S5Stoch == null) continue;

                var M1Stoch = stochasticM1.GetValue(dateTime);
                if (M1Stoch == null) continue;

                var M15Stoch = stochasticM15.GetValue(dateTime);
                if (M15Stoch == null) continue;

                var expected = winpip.GetValue(dateTime);
                if (expected == null) continue;

                dynamic record = new ExpandoObject();
                record.Time = S5Candle.DateTime.TimeOfDay.TotalSeconds;

                record.S5V = S5Candle.Volume;
                record.S5C = S5Candle.CloseRelativ;
                record.S5H = S5Candle.HighRaltiv;
                record.S5L = S5Candle.LowRelativ;
                record.S5StochK = S5Stoch.K;
                record.S5StochD = S5Stoch.D;

                record.M1V = M1Candle.Volume;
                record.M1C = M1Candle.CloseRelativ;
                record.M1H = M1Candle.HighRaltiv;
                record.M1L = M1Candle.LowRelativ;
                record.M1StochK = M1Stoch.K;
                record.M1StochD = M1Stoch.D;

                record.M15V = M15Candle.Volume;
                record.M15C = M15Candle.CloseRelativ;
                record.M15H = M15Candle.HighRaltiv;
                record.M15L = M15Candle.LowRelativ;
                record.M15StochK = M15Stoch.K;
                record.M15StochD = M15Stoch.D;

                record.FPL = expected.Long;
                //record.FPS = expected.Short;

                records.Add(record);
            }

            using (var writer = new StreamWriter("train.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.Delimiter = ",";
                csv.WriteRecords(records);
            }
        }
    }
}
