using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using CsvHelper;

namespace FxEngine.Library
{
    public static class Oanda
    {
        private static string _token = "b8346e2d708eeb99bd94da4cd418392e-ba2b8c33453b9cdb418ab0400d45246e";

        private static string practice = "https://api-fxpractice.oanda.com";

        private static OandaCandles GetCandles(string instrument,string granularity, DateTimeOffset from)
        {

            string url = $"{practice}/v3/instruments/{instrument}/candles?price=M&granularity={granularity}&from={from.ToRfc3339()}&count=5000";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers["Authorization"] = $"Bearer {_token}";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string json;

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                json = sr.ReadToEnd();
            }

            var result = OandaCandles.FromJson(json);
            return result;
        }


        public static OandaCandle[] LoadLiveCandles(string instrument, string granularity, int count = 5000)
        {
            string url = $"{practice}/v3/instruments/{instrument}/candles?price=M&granularity={granularity}&count={count}";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers["Authorization"] = $"Bearer {_token}";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string json;

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                json = sr.ReadToEnd();
            }

            var result = OandaCandles.FromJson(json);
            return result.Candles;
        }

        public static OandaCandle[] LoadCandles(string instrument, string granularity, DateTime from)
        {
            List<OandaCandle> result = new List<OandaCandle>();


            DateTimeOffset date = from.ToUniversalTime().Date;

            while (date.Date < DateTimeOffset.Now.AddDays(-1).Date )
            {

                string filename = $"{instrument}{granularity}{date.Year}{date.Month.ToString("D2")}{date.Day.ToString("D2")}.csv";

                if (!File.Exists(filename))
                {
                    DateTimeOffset iterator = date;

                    //date = date.AddSeconds(1);
                    List<OandaCandle> daycandles = new List<OandaCandle>();
                    do
                    {

                        var candles = GetCandles(instrument, granularity, iterator.DateTime);
                        daycandles.AddRange(candles.Candles);
                        Console.WriteLine($" receive {candles.Candles.Count()} from {candles.Candles.Min(c => c.Time)} to {candles.Candles.Max(c => c.Time)}");
                        var max = daycandles.Max(c => c.Time);
                        Console.WriteLine($"max date {max}");
                        iterator = DateTimeExtension.Max(date, candles.Candles.Max(c => c.Time));
                    } while (iterator.Date < date.Date.AddDays(1));

                    daycandles = daycandles.Where(c => c.Time.Date == date.Date).ToList();
                    if(daycandles.Count() > 0)
                    {
                        Console.WriteLine($" loaded {daycandles.Count} from {daycandles.Min(c => c.Time)} to {daycandles.Max(c => c.Time)}");
                        using (var writer = new StreamWriter(filename))
                        using (var csv = new CsvWriter(writer))
                        {
                            csv.WriteRecords(daycandles);
                        }

                        result.AddRange(daycandles);

                    }
                    
                }
                else
                {
                    using (var reader = new StreamReader(filename))
                    using (var csv = new CsvReader(reader))
                    {
                        var records = csv.GetRecords<OandaCandle>();
                        result.AddRange(records);
                    }
                }

                date = date.AddDays(1);
            }

            return result.ToArray();
        }
    }

    public partial class OandaCandles
    {
        [JsonProperty("instrument")]
        public string Instrument { get; set; }

        [JsonProperty("granularity")]
        public string Granularity { get; set; }

        [JsonProperty("candles")]
        public OandaCandle[] Candles { get; set; }
    }

    public partial class OandaCandle
    {
        [JsonProperty("complete")]
        public bool Complete { get; set; }

        [JsonProperty("volume")]
        public int Volume { get; set; }

        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }

        [JsonProperty("mid")]
        public Mid Mid { get; set; }
    }

    public partial class Mid
    {
        [JsonProperty("o")]
        public decimal O { get; set; }

        [JsonProperty("h")]
        public decimal H { get; set; }

        [JsonProperty("l")]
        public decimal L { get; set; }

        [JsonProperty("c")]
        public decimal C { get; set; }
    }

    public partial class OandaCandles
    {
        public static OandaCandles FromJson(string json) => JsonConvert.DeserializeObject<OandaCandles>(json, FxEngine.Library.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this OandaCandles self) => JsonConvert.SerializeObject(self, FxEngine.Library.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
