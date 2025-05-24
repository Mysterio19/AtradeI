using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtradeI.Models
{
    public class DecisionResponse
    {
        public string Pair { get; set; }
        public string Reason { get; set; }
        public string Confidence { get; set; }
        public string Analyzed_Data { get; set; }
        public string Decision { get; set; }
        public string Current_Price { get; set; }
        public DateTime Time { get; set; }
    }
}
