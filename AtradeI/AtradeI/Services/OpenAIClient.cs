using AtradeI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AtradeI.Services
{
    public class OpenAIClient
    {
        private readonly HttpClient _http;
        private readonly string _openAiApiKey;

        public OpenAIClient(HttpClient httpClient, string apiKey)
        {
            _http = httpClient;
            _openAiApiKey = apiKey;
        }

        public async Task<DecisionResponse> GetDecisionAsync(TradeContext context)
        {
            var json = JsonSerializer.Serialize(context, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var requestBody = new
            {
                model = "gpt-4o",
                temperature = 0.2,
                messages = new[]
                {
                    new { role = "system", content = "You are a crypto trading assistant. Based on the data, return json in the next format:" +
                    "{\r\n\"pair\": \"*\",\r\n\"reason\" : \"*\",\r\n\"confidence\": \"*\",\r\n\"analyzed_data\": \"*\",\r\n\"decision\": \"*\",\r\n\"current_price\": \"*\",\r\n\"time\": \"*\"\r\n}" +
                    "reason should widely describe the reason of your decsision, max 500 symbols (describe in details your decision)," +
                    " confidence in precents," +
                    " analyzed data is data the decision based on such as provided historical data, new, indicators ( less than 50 symbols )," +
                    " decision is \"BUY, SELL or HOLD\"." +
                    " current_price is the price at the moment of decision" +
                    " time is the time at the moment of decision in UTC-format" +
                    " Use web search to find additional needed data about market and all the pair related data." +
                    "Also use smart money concept for analysis." },
                    new { role = "user", content = json }
                }
            };

            var reqJson = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

            var resp = await _http.PostAsync("https://api.openai.com/v1/chat/completions", reqJson);
            var resultJson = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(resultJson);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            try
            {
                var parsedResult = JsonSerializer.Deserialize<DecisionResponse>(content);
                Console.WriteLine($"Decision: {parsedResult.Decision} | Confidence: {parsedResult.Confidence} | Reason: {parsedResult.Reason}");
                return parsedResult;
            }
            catch
            {
                Console.WriteLine("⚠️ Failed to parse response: " + content);
                return new DecisionResponse
                {
                    Decision = "FAILED",
                };
            }
        }
    }
}
