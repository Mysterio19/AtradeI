using AtradeI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

        public async Task<List<Candle>> GetCandlesAsync(string symbol)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var from = now - 60 * 60 * 24;
            var url = $"https://api.bybit.com/v5/market/kline?category=linear&symbol={symbol}&interval=15&start={from * 1000}&end={now * 1000}&limit=100";

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

            return candles;
        }
    }
}
