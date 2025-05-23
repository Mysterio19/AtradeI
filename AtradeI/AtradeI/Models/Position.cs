using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtradeI.Models
{
    public class Position
    {
        public string Symbol { get; set; }
        public string Side { get; set; }
        public double Entry { get; set; }
        public double Qty { get; set; }
        public double Pnl { get; set; }
    }
}
