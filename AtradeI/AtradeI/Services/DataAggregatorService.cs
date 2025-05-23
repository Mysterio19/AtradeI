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

        public DataAggregatorService(
            CandleService candles,
            IndicatorCalculator indicators,
            NewsFetcher news,
            GlobalMetricsService metrics,
            BybitPositionService positions)
        {
            _candles = candles;
            _indicators = indicators;
            _news = news;
            _metrics = metrics;
            _positions = positions;
        }

        public async Task<TradeContext> BuildContextAsync(string symbol)
        {
            var context = new TradeContext { Symbol = symbol };
            var candles = await _candles.GetCandlesAsync(symbol);
            context.Indicators = _indicators.ComputeIndicators(candles);
            context.News = await _news.GetNewsHeadlinesAsync();
            context.GlobalMetrics = await _metrics.GetGlobalMetricsAsync();
           // context.Positions = await _positions.GetPositionsAsync(symbol);
            context.MarketStructure = InferMarketStructure(context.Indicators);
            return context;
        }

        private MarketStructure InferMarketStructure(Indicators indicators)
        {
            var volatility = indicators.ATR > 100 ? "increasing" : "stable";
            var pattern = indicators.RSI > 70 ? "Overbought" : indicators.RSI < 30 ? "Oversold" : "Neutral";
            return new MarketStructure { Volatility = volatility, Pattern = pattern };
        }
    }
}
