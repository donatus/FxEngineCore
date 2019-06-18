using CsvHelper;
using FxEngine.ConsoleML.Model.DataModels;
using FxEngine.Library;
using FxEngine.ML;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FxEngine.Console
{
    class Program
    {

        static void Main(string[] args)
        {
            var candles = Oanda.LoadCandles("EUR_USD", "S5", DateTime.Now.AddDays(-45));

            var collection = new CandleCollection("EUR_USD", Period.S, 5);
            var M1Consolditation = new ConsolidatedCandleCollection(collection, Period.M, 1);
            var winpip = new PipExpectation(collection, 60);

            foreach (OandaCandle candle in candles.Where(c => c.Time < DateTime.Now.AddDays(-10)))
            {
                DateTime datetime = new DateTime(candle.Time.Year, candle.Time.Month, candle.Time.Day, candle.Time.Hour, candle.Time.Minute, candle.Time.Second);
                collection.AddCandle(datetime, candle.Mid.O, candle.Mid.C, candle.Mid.H, candle.Mid.L, candle.Volume);
            }

            FeaturesFactory features = new FeaturesFactory();
            features.AddFeaturable(collection);
            features.AddFeaturable(M1Consolditation);
            features.AddPredition(winpip);

            

            MLContext mlContext = new MLContext(seed: 0);
            var pipeline = features.Transform(mlContext);
            var trainedmodel = pipeline.

            PrevisionTradeStrategy strategy = new PrevisionTradeStrategy(collection,winpip);

            TradeSimulation simulation = new TradeSimulation(collection, strategy);

            DateTime from = DateTime.Now.AddDays(-7).Date;
            DateTime to = DateTime.Now.Date;
            simulation.Simulate(from, to);
        }

        static void Main2(string[] args)
        {
            System.Console.WriteLine("Loading model ");
            MLContext mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load("MLModel.zip", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
            System.Console.Write("[OK]\n");


            

            System.Console.Write("Loading candles ");
            var candlesbefore = Oanda.LoadCandles("EUR_USD", "S5", DateTime.Now.AddDays(-2));
            var candles = Oanda.LoadLiveCandles("EUR_USD", "S5");
            System.Console.Write($"[OK {candles.Length}]\n");

            System.Console.Write("Analyse data ");
            var collection = new CandleCollection("EUR_USD", Period.S, 5);
            var minuteconsolidation = new ConsolidatedCandleCollection(collection, Period.M, 1);
            var m15Consolidation = new ConsolidatedCandleCollection(collection, Period.M, 15);
            var stochasticS5 = new StochasticStats(collection);
            var stochasticM1 = new StochasticStats(minuteconsolidation);
            var stochasticM15 = new StochasticStats(m15Consolidation);
            var winpip = new PipExpectation(collection, 30);

            foreach (OandaCandle candle in candlesbefore)
            {
                DateTime datetime = new DateTime(candle.Time.Year, candle.Time.Month, candle.Time.Day, candle.Time.Hour, candle.Time.Minute, candle.Time.Second);
                collection.AddCandle(datetime, candle.Mid.O, candle.Mid.C, candle.Mid.H, candle.Mid.L, candle.Volume);
            }

            foreach (OandaCandle candle in candles)
            {
                DateTime datetime = new DateTime(candle.Time.Year, candle.Time.Month, candle.Time.Day, candle.Time.Hour, candle.Time.Minute, candle.Time.Second);
                collection.AddCandle(datetime, candle.Mid.O, candle.Mid.C, candle.Mid.H, candle.Mid.L, candle.Volume);
            }


            System.Console.Write("[OK]\n");

            System.Console.Write("Prediciction ");
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

                ModelInput input = new ModelInput()
                {
                    Time = (float)S5Candle.DateTime.TimeOfDay.TotalSeconds,
                    Decision = 0,
                    S5V = S5Candle.Volume,
                    S5C = decimal.ToSingle(S5Candle.CloseRelativ),
                    S5H = decimal.ToSingle(S5Candle.HighRaltiv),
                    S5L = decimal.ToSingle(S5Candle.LowRelativ),
                    S5StochK = (float)(S5Stoch.K),
                    S5StochD = (float)(S5Stoch.D),
                    M1V = M1Candle.Volume,
                    M1C = decimal.ToSingle(M1Candle.CloseRelativ),
                    M1H = decimal.ToSingle(M1Candle.HighRaltiv),
                    M1L = decimal.ToSingle(M1Candle.LowRelativ),
                    M1StochK = (float)M1Stoch.K,
                    M1StochD = (float)M1Stoch.D,
                    M15V = M15Candle.Volume,
                    M15C = decimal.ToSingle(M15Candle.CloseRelativ),
                    M15H = decimal.ToSingle(M15Candle.HighRaltiv),
                    M15L = decimal.ToSingle(M15Candle.LowRelativ),
                    M15StochK = (float)M15Stoch.K,
                    M15StochD = (float)M15Stoch.D
                };

                ModelOutput result = predEngine.Predict(input);

                var expected = winpip.GetValue(dateTime);
                if (expected == null) continue;

                float decision = 0f;

                if (expected.Long > 5m && expected.Short < 1.6m)
                {
                    decision = 1;
                }
                else if (expected.Short > 5m && expected.Long < 1.6m)
                {
                    decision = 2;
                }
                else
                {
                    decision = 0;
                }

                if (result.Prediction > 0 || decision > 0)
                    System.Console.WriteLine($"[{dateTime}] Decision {decision} result {result.Prediction} Predicted scores: [{String.Join(",", result.Score)}]");
                else if(result.Prediction > 0)
                {
                    System.Console.WriteLine($"[{dateTime}] Operation {result.Prediction} Predicted scores: [{String.Join(",", result.Score)}]");
                }
            }

            while (true)
            {
                Task.Delay(2000).Wait();
                var candleslive = Oanda.LoadLiveCandles("EUR_USD", "S5",2);
                foreach (OandaCandle candle in candleslive)
                {
                    DateTime datetime = new DateTime(candle.Time.Year, candle.Time.Month, candle.Time.Day, candle.Time.Hour, candle.Time.Minute, candle.Time.Second);
                    collection.AddCandle(datetime, candle.Mid.O, candle.Mid.C, candle.Mid.H, candle.Mid.L, candle.Volume);
                }

                foreach (DateTime dateTime in collection._list.Keys.OrderByDescending(c => c.Ticks).Take(2))
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

                    ModelInput input = new ModelInput()
                    {
                        Time = (float)S5Candle.DateTime.TimeOfDay.TotalSeconds,
                        Decision = 0,
                        S5V = S5Candle.Volume,
                        S5C = decimal.ToSingle(S5Candle.CloseRelativ),
                        S5H = decimal.ToSingle(S5Candle.HighRaltiv),
                        S5L = decimal.ToSingle(S5Candle.LowRelativ),
                        S5StochK = (float)(S5Stoch.K),
                        S5StochD = (float)(S5Stoch.D),
                        M1V = M1Candle.Volume,
                        M1C = decimal.ToSingle(M1Candle.CloseRelativ),
                        M1H = decimal.ToSingle(M1Candle.HighRaltiv),
                        M1L = decimal.ToSingle(M1Candle.LowRelativ),
                        M1StochK = (float)M1Stoch.K,
                        M1StochD = (float)M1Stoch.D,
                        M15V = M15Candle.Volume,
                        M15C = decimal.ToSingle(M15Candle.CloseRelativ),
                        M15H = decimal.ToSingle(M15Candle.HighRaltiv),
                        M15L = decimal.ToSingle(M15Candle.LowRelativ),
                        M15StochK = (float)M15Stoch.K,
                        M15StochD = (float)M15Stoch.D
                    };

                    ModelOutput result = predEngine.Predict(input);


                    System.Console.WriteLine($"[{dateTime}] Operation {result.Prediction} Predicted scores: [{String.Join(",", result.Score)}]");

                }

            }
        }

        static void Main3(string[] args)
        {
            

            var candles = Oanda.LoadCandles("EUR_USD", "S5", DateTime.Now.AddDays(-45));

            var collection = new CandleCollection("EUR_USD", Period.S, 5);
            var minuteconsolidation = new ConsolidatedCandleCollection(collection, Period.M, 1);
            var m15Consolidation = new ConsolidatedCandleCollection(collection, Period.M, 15);
            var h1consolidation = new ConsolidatedCandleCollection(collection, Period.H, 1);
            var stochasticS5 = new StochasticStats(collection);
            var stochasticM1 = new StochasticStats(minuteconsolidation);
            var stochasticM15 = new StochasticStats(m15Consolidation);
            var h1stoch = new StochasticStats(h1consolidation);
            var winpip = new PipExpectation(collection, 60);
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

                var H1Candle = h1consolidation.GetCandle(dateTime);
                if (H1Candle == null) continue;

                var S5Stoch = stochasticS5.GetValue(dateTime);
                if (S5Stoch == null) continue;

                var M1Stoch = stochasticM1.GetValue(dateTime);
                if (M1Stoch == null) continue;

                var M15Stoch = stochasticM15.GetValue(dateTime);
                if (M15Stoch == null) continue;

                var H1Stoch = h1stoch.GetValue(dateTime);
                if (H1Stoch == null) continue;

                var expected = winpip.GetValue(dateTime);
                if (expected == null) continue;

                dynamic record = new ExpandoObject();
                record.Time = S5Candle.DateTime.TimeOfDay.TotalSeconds;

                record.S5V = S5Candle.Volume;
                record.S5C = decimal.ToSingle(S5Candle.CloseRelativ);
                record.S5H = decimal.ToSingle(S5Candle.HighRaltiv);
                record.S5L = decimal.ToSingle(S5Candle.LowRelativ);
                record.S5StochK = (float)S5Stoch.K;
                record.S5StochD = (float)S5Stoch.D;

                record.M1V = M1Candle.Volume;
                record.M1C = decimal.ToSingle(M1Candle.CloseRelativ);
                record.M1H = decimal.ToSingle(M1Candle.HighRaltiv);
                record.M1L = decimal.ToSingle(M1Candle.LowRelativ);
                record.M1StochK = (float)M1Stoch.K;
                record.M1StochD = (float)M1Stoch.D;

                record.M15V = M15Candle.Volume;
                record.M15C = decimal.ToSingle(M15Candle.CloseRelativ);
                record.M15H = decimal.ToSingle(M15Candle.HighRaltiv);
                record.M15L = decimal.ToSingle(M15Candle.LowRelativ);
                record.M15StochK = (float)M15Stoch.K;
                record.M15StochD = (float)M15Stoch.D;

                record.H1V = H1Candle.Volume;
                record.H1C = decimal.ToSingle(H1Candle.CloseRelativ);
                record.H1H = decimal.ToSingle(H1Candle.HighRaltiv);
                record.H1L = decimal.ToSingle(H1Candle.LowRelativ);
                record.H1StochK = (float)H1Stoch.K;
                record.H1StochD = (float)H1Stoch.D;

                //record.FPL = expected.Long;
                //record.FPS = expected.Short;

                if (expected.Long > 5m)
                {
                    record.Decision = 1;
                }
                //else if (expected.Long > 2m)
                //{
                //    record.Decision = 1;
                //}
                else
                {
                    record.Decision = 0;
                }

                records.Add(record);
            }

            using (var writer = new StreamWriter("train.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.Delimiter = ",";
                csv.WriteRecords(records);
            }

            MLContext mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load("MLModel.zip", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            foreach (DateTime dateTime in collection._list.Keys.OrderBy(c => c.Ticks))
            {
                var S5Candle = collection.GetCandle(dateTime);
                if (S5Candle == null) continue;

                var M1Candle = minuteconsolidation.GetCandle(dateTime);
                if (M1Candle == null) continue;

                var M15Candle = m15Consolidation.GetCandle(dateTime);
                if (M15Candle == null) continue;

                var H1Candle = h1consolidation.GetCandle(dateTime);
                if (H1Candle == null) continue;

                var S5Stoch = stochasticS5.GetValue(dateTime);
                if (S5Stoch == null) continue;

                var M1Stoch = stochasticM1.GetValue(dateTime);
                if (M1Stoch == null) continue;

                var M15Stoch = stochasticM15.GetValue(dateTime);
                if (M15Stoch == null) continue;

                var H1Stoch = h1stoch.GetValue(dateTime);
                if (H1Stoch == null) continue;

                var expected = winpip.GetValue(dateTime);
                if (expected == null) continue;

                float decision = 0f;

                if (expected.Long > 5m && expected.Short < 1.6m)
                {
                    decision = 1;
                }
                else if (expected.Short > 5m && expected.Long < 1.6m)
                {
                    decision = 2;
                }
                else
                {
                    decision = 0;
                }

                
                ModelInput input = new ModelInput()
                {
                    Time = (float)S5Candle.DateTime.TimeOfDay.TotalSeconds,
                    Decision = 0,
                    S5V = S5Candle.Volume,
                    S5C = decimal.ToSingle(S5Candle.CloseRelativ),
                    S5H = decimal.ToSingle(S5Candle.HighRaltiv),
                    S5L = decimal.ToSingle(S5Candle.LowRelativ),
                    S5StochK = (float)(S5Stoch.K),
                    S5StochD = (float)(S5Stoch.D),
                    M1V = M1Candle.Volume,
                    M1C = decimal.ToSingle(M1Candle.CloseRelativ),
                    M1H = decimal.ToSingle(M1Candle.HighRaltiv),
                    M1L = decimal.ToSingle(M1Candle.LowRelativ),
                    M1StochK = (float)M1Stoch.K,
                    M1StochD = (float)M1Stoch.D,
                    M15V = M15Candle.Volume,
                    M15C = decimal.ToSingle(M15Candle.CloseRelativ),
                    M15H = decimal.ToSingle(M15Candle.HighRaltiv),
                    M15L = decimal.ToSingle(M15Candle.LowRelativ),
                    M15StochK = (float)M15Stoch.K,
                    M15StochD = (float)M15Stoch.D
                };

                ModelOutput result = predEngine.Predict(input);
                if(result.Prediction > 0 || input.Decision > 0 )
                    System.Console.WriteLine($"[{input.Time}] decision {input.Decision} result {result.Prediction} Predicted scores: [{String.Join(",", result.Score)}]");
            }
        }

        public void ConsumeModel(ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load("MLModel.zip", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            // Use the code below to add input data
            // input.

            // Try model on sample data
            ModelOutput result = predEngine.Predict(input);
        }

    }
}
