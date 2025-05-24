using AtradeI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtradeI.Services
{
    public class DataAggregatorService
    {
        private readonly CandleService _candles;
        private readonly IndicatorCalculator _indicators;
        private readonly NewsFetcher _news;
        private readonly GlobalMetricsService _metrics;
        private readonly BybitPositionService _positions;
        private readonly MultiTimeframeService _multiTimeframe;

        public DataAggregatorService(
            CandleService candles,
            IndicatorCalculator indicators,
            NewsFetcher news,
            GlobalMetricsService metrics,
            BybitPositionService positions,
            MultiTimeframeService multiTimeframe)
        {
            _candles = candles;
            _indicators = indicators;
            _news = news;
            _metrics = metrics;
            _positions = positions;
            _multiTimeframe = multiTimeframe;
        }

        public async Task<TradeContext> BuildContextAsync(string symbol)
        {
            var context = new TradeContext { Symbol = symbol };
            context.News = await _news.GetNewsHeadlinesAsync();
            context.GlobalMetrics = await _metrics.GetGlobalMetricsAsync();
            context.Timeframes = await _multiTimeframe.BuildMultiTimeframeAnalysisAsync(symbol);
            // context.Positions = await _positions.GetPositionsAsync(symbol);
            return context;
        }
    }
}
