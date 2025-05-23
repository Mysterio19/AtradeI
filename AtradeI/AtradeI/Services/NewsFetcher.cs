using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AtradeI.Services
{
    public class NewsFetcher
    {
        private readonly HttpClient _http;

        public NewsFetcher(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<List<string>> GetNewsHeadlinesAsync()
        {
            var headlines = new List<string>();
            try
            {
                var rss = await _http.GetStreamAsync("https://www.coindesk.com/arc/outboundfeeds/rss/");
                using var reader = XmlReader.Create(rss);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "title")
                    {
                        var title = reader.ReadElementContentAsString();
                        if (!string.IsNullOrWhiteSpace(title) && !title.Contains("CoinDesk"))
                            headlines.Add(title);
                        if (headlines.Count >= 5)
                            break;
                    }
                }
            }
            catch
            {
                headlines.Add("News fetch failed");
            }
            return headlines;
        }
    }
}
