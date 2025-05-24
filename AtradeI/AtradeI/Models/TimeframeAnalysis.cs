using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtradeI.Models
{
    public class TimeframeAnalysis
    {
        public string IntervalInMinutes { get; set; }
        public int Days { get; set; }
        public Indicators Indicators { get; set; }
        public string Summary { get; set; }
        public MarketStructure MarketStructure { get; set; }
    }

}
