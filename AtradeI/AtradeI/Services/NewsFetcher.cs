using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace AtradeI.Services
{
    public class NewsFetcher
    {
        private readonly HttpClient _http;
        private readonly string[] _sources = new[]
        {
            "https://www.coindesk.com/arc/outboundfeeds/rss/",
            "https://cointelegraph.com/rss",
            "https://cryptoslate.com/feed/"
        };

        private const int MaxHeadlines = 20;

        public NewsFetcher(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<List<string>> GetNewsHeadlinesAsync()
        {
            var headlines = new HashSet<string>();

            foreach (var url in _sources)
            {
                try
                {
                    var rss = await _http.GetStreamAsync(url);
                    using var reader = XmlReader.Create(rss);
                    while (reader.Read() && headlines.Count < MaxHeadlines)
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "title")
                        {
                            var title = reader.ReadElementContentAsString();
                            if (!string.IsNullOrWhiteSpace(title) && !IsSourceTitle(title))
                                headlines.Add(title.Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to fetch news from {url}: {ex.Message}");
                }
            }

            if (!headlines.Any())
                headlines.Add("⚠️ News fetch failed");

            return headlines.ToList();
        }

        private bool IsSourceTitle(string title)
        {
            return title.Contains("CoinDesk", StringComparison.OrdinalIgnoreCase)
                || title.Contains("Cointelegraph", StringComparison.OrdinalIgnoreCase)
                || title.Contains("CryptoSlate", StringComparison.OrdinalIgnoreCase);
        }
    }
}
