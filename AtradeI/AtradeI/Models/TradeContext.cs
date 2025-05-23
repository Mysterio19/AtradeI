using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtradeI.Models
{
    public class TradeContext
    {
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Symbol { get; set; } = "BTCUSDT";
        public Indicators Indicators { get; set; }
        public MarketStructure MarketStructure { get; set; }
        public List<string> News { get; set; }
        public GlobalMetrics GlobalMetrics { get; set; }
        public List<Position> Positions { get; set; }
    }
}
