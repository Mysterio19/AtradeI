using AtradeI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AtradeI.Services
{
    public class CandleService
    {
        private readonly HttpClient _http;

        public CandleService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<List<Candle>> GetCandlesAsync(string symbol, string interval, int days)
        {
            var allCandles = new List<Candle>();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var intervalSec = IntervalToSeconds(interval);
            var totalSeconds = days * 86400;
            var end = now;

            while (totalSeconds > 0)
            {
                var fetchSpan = Math.Min(totalSeconds, intervalSec * 1000); // <= 1000 свечей за раз
                var from = end - fetchSpan;
                var url = $"https://api.bybit.com/v5/market/kline?category=linear&symbol={symbol}&interval={interval}&start={from * 1000}&end={end * 1000}&limit=1000";

                var response = await _http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                var list = doc.RootElement.GetProperty("result").GetProperty("list");
                var candles = new List<Candle>();
                foreach (var el in list.EnumerateArray())
                {
                    candles.Add(new Candle
                    {
                        Timestamp = long.Parse(el[0].GetString() ?? "0"),
                        Open = double.Parse(el[1].GetString() ?? "0", CultureInfo.InvariantCulture),
                        High = double.Parse(el[2].GetString() ?? "0", CultureInfo.InvariantCulture),
                        Low = double.Parse(el[3].GetString() ?? "0", CultureInfo.InvariantCulture),
                        Close = double.Parse(el[4].GetString() ?? "0", CultureInfo.InvariantCulture),
                        Volume = double.Parse(el[5].GetString() ?? "0", CultureInfo.InvariantCulture)
                    });
                }

                if (candles.Count == 0)
                    break;

                allCandles.AddRange(candles);
                end = candles.First().Timestamp / 1000;
                totalSeconds -= fetchSpan;
            }

            allCandles.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

            var expectedCount = days * 86400 / intervalSec;
            Console.WriteLine($"[{symbol} {interval}] Expected: {expectedCount} candles, Retrieved: {allCandles.Count}");

            if (allCandles.Count < expectedCount * 0.8)
            {
                throw new Exception($"⚠️ Retrieved too few candles for {symbol} {interval}: expected {expectedCount}, got {allCandles.Count}");
            }
            return allCandles;
        }

        private int IntervalToSeconds(string interval)
        {
            return interval switch
            {
                "1" => 60,
                "3" => 180,
                "5" => 300,
                "15" => 900,
                "30" => 1800,
                "60" => 3600,
                "120" => 7200,
                "240" => 14400,
                "360" => 21600,
                "720" => 43200,
                "D" => 86400,
                "W" => 604800,
                "M" => 2592000,
                _ => throw new ArgumentException("Unsupported interval: " + interval)
            };
        }
    }
}
