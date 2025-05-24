using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtradeI.Models;

namespace AtradeI.Services
{
    public class MultiTimeframeService
    {
        private readonly CandleService _candleService;
        private readonly IndicatorCalculator _indicatorCalculator;

        public MultiTimeframeService(CandleService candleService, IndicatorCalculator indicatorCalculator)
        {
            _candleService = candleService;
            _indicatorCalculator = indicatorCalculator;
        }

        private readonly List<(string interval, int days)> _timeframes = new()
        {
            ("15", 2),     // 15m for 2 days
            ("60", 7),     // 1h for 7 days
            ("240", 30),   // 4h for 30 days
            ("D", 2000),// 1d for 2000 days
        };

        public async Task<List<TimeframeAnalysis>> BuildMultiTimeframeAnalysisAsync(string symbol)
        {
            var results = new List<TimeframeAnalysis>();

            foreach (var (interval, days) in _timeframes)
            {
                var candles = await _candleService.GetCandlesAsync(symbol, interval, days);

                var indicators = _indicatorCalculator.ComputePartialIndicators(candles);


                var summary = $"RSI={(double.IsNaN(indicators.RSI) ? "N/A" : Math.Round(indicators.RSI, 1))}, " +
                              $"EMA9={(double.IsNaN(indicators.EMA9) ? "N/A" : Math.Round(indicators.EMA9, 2))}, " +
                              $"EMA50={(double.IsNaN(indicators.EMA50) ? "N/A" : Math.Round(indicators.EMA50, 2))}, " +
                              $"MACD={(indicators.MACD == null ? "N/A" : Math.Round(indicators.MACD.Line, 2) + "/" + Math.Round(indicators.MACD.Signal, 2))}, " +
                              $"ATR={(double.IsNaN(indicators.ATR) ? "N/A" : Math.Round(indicators.ATR, 2))}";

                results.Add(new TimeframeAnalysis
                {
                    IntervalInMinutes = interval,
                    Days = days,
                    Indicators = indicators,
                    Summary = summary,
                    MarketStructure = new MarketStructure
                    {
                        Volatility = double.IsNaN(indicators.ATR) ? "unknown" : indicators.ATR > 100 ? "increasing" : "stable",
                        Pattern = double.IsNaN(indicators.RSI) ? "unknown" : indicators.RSI > 70 ? "Overbought" : indicators.RSI < 30 ? "Oversold" : "Neutral"
                    }
                });
            }

            return results;
        }
    }

}
