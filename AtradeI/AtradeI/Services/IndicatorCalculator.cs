using AtradeI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtradeI.Services
{
    public class IndicatorCalculator
    {
        public Indicators ComputeIndicators(List<Candle> candles)
        {
            var closes = candles.Select(c => c.Close).ToList();
            var ema9 = EMA(closes, 9);
            var ema50 = EMA(closes, 50);
            var rsi = RSI(closes, 14);
            var (macdLine, signalLine) = MACD(closes);
            var atr = ATR(candles, 14);

            return new Indicators
            {
                RSI = rsi,
                MACD = new Macd { Line = macdLine, Signal = signalLine },
                EMA9 = ema9.LastOrDefault(),
                EMA50 = ema50.LastOrDefault(),
                ATR = atr
            };
        }

        private List<double> EMA(List<double> values, int period)
        {
            var result = new List<double>();
            double multiplier = 2.0 / (period + 1);
            result.Add(values.Take(period).Average());

            for (int i = period; i < values.Count; i++)
            {
                double ema = (values[i] - result.Last()) * multiplier + result.Last();
                result.Add(ema);
            }
            return result;
        }

        private double RSI(List<double> values, int period)
        {
            double gain = 0, loss = 0;
            for (int i = 1; i <= period; i++)
            {
                var diff = values[i] - values[i - 1];
                if (diff >= 0) gain += diff; else loss -= diff;
            }
            gain /= period;
            loss /= period;
            if (loss == 0) return 100;
            double rs = gain / loss;
            return 100 - (100 / (1 + rs));
        }

        private (double, double) MACD(List<double> values)
        {
            var ema12 = EMA(values, 12);
            var ema26 = EMA(values, 26);
            var macdLine = ema12.Zip(ema26, (a, b) => a - b).ToList();
            var signalLine = EMA(macdLine, 9);
            return (macdLine.LastOrDefault(), signalLine.LastOrDefault());
        }

        private double ATR(List<Candle> candles, int period)
        {
            var trs = new List<double>();
            for (int i = 1; i < candles.Count; i++)
            {
                var high = candles[i].High;
                var low = candles[i].Low;
                var prevClose = candles[i - 1].Close;
                var tr = Math.Max(high - low, Math.Max(Math.Abs(high - prevClose), Math.Abs(low - prevClose)));
                trs.Add(tr);
            }
            return trs.Skip(trs.Count - period).Average();
        }
    }
}
