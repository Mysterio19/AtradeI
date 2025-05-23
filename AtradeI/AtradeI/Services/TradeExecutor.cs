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
    public class TradeExecutor
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public TradeExecutor(HttpClient httpClient, string apiKey, string apiSecret)
        {
            _http = httpClient;
            _apiKey = apiKey;
            _apiSecret = apiSecret;
        }

        public async Task<bool> ExecuteOrderAsync(string symbol, string side, decimal qty)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var body = JsonSerializer.Serialize(new
            {
                category = "linear",
                symbol = symbol,
                side = side.ToUpper(),
                orderType = "Market",
                qty = qty.ToString(CultureInfo.InvariantCulture),
                timeInForce = "GTC"
            });

            var sign = Sign(ts + _apiKey + "5000" + body);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.bybit.com/v5/order/create")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-BYBIT-API-KEY", _apiKey);
            request.Headers.Add("X-BYBIT-API-SIGN", sign);
            request.Headers.Add("X-BYBIT-API-TIMESTAMP", ts);
            request.Headers.Add("X-BYBIT-API-RECV-WINDOW", "5000");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("ORDER RESPONSE: " + json);

            return response.IsSuccessStatusCode;
        }

        private string Sign(string payload)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
