using AtradeI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AtradeI.Services
{
    public class BybitPositionService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public BybitPositionService(HttpClient httpClient, string apiKey, string apiSecret)
        {
            _http = httpClient;
            _apiKey = apiKey;
            _apiSecret = apiSecret;
        }

        public async Task<List<Position>> GetPositionsAsync(string symbol)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var body = $"{{\"category\":\"linear\",\"symbol\":\"{symbol}\"}}";
            var sign = Sign(ts + _apiKey + "5000" + body);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.bybit.com/v5/position/list")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-BYBIT-API-KEY", _apiKey);
            request.Headers.Add("X-BYBIT-API-SIGN", sign);
            request.Headers.Add("X-BYBIT-API-TIMESTAMP", ts);
            request.Headers.Add("X-BYBIT-API-RECV-WINDOW", "5000");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var list = doc.RootElement.GetProperty("result").GetProperty("list");

            var positions = new List<Position>();
            foreach (var item in list.EnumerateArray())
            {
                positions.Add(new Position
                {
                    Symbol = item.GetProperty("symbol").GetString(),
                    Side = item.GetProperty("side").GetString(),
                    Entry = double.Parse(item.GetProperty("entryPrice").GetString(), CultureInfo.InvariantCulture),
                    Qty = double.Parse(item.GetProperty("size").GetString(), CultureInfo.InvariantCulture),
                    Pnl = double.Parse(item.GetProperty("unrealisedPnl").GetString(), CultureInfo.InvariantCulture)
                });
            }

            return positions;
        }

        private string Sign(string payload)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
