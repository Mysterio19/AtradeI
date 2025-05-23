using AtradeI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AtradeI.Services
{
    public class GlobalMetricsService
    {
        private readonly HttpClient _http;

        public GlobalMetricsService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<GlobalMetrics> GetGlobalMetricsAsync()
        {
            var resp = await _http.GetStringAsync("https://api.coingecko.com/api/v3/global");
            using var doc = JsonDocument.Parse(resp);
            var data = doc.RootElement.GetProperty("data");
            return new GlobalMetrics
            {
                BtcDominance = data.GetProperty("market_cap_percentage").GetProperty("btc").GetDouble(),
                TotalCap = data.GetProperty("total_market_cap").GetProperty("usd").GetDouble()
            };
        }
    }
}
