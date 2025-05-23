using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtradeI.Models
{
    public class Indicators
    {
        public double RSI { get; set; }
        public Macd MACD { get; set; }
        public double EMA9 { get; set; }
        public double EMA50 { get; set; }
        public double ATR { get; set; }
    }

}
