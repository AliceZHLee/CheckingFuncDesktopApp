using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICEFixAdapter.Models {
    public class CombiDefinition {
        public int CombiPercentage { get; set; }
        public string CombiPriceBasis { get; set; }
        public string CombiPriceBasisPeriod { get; set; }//Returns the Platts basis period ex: Full month, quarter, 5 days, etc.
        public string CombiPriceBasisSubLevel { get; set; }
        public decimal CombiLegPrice { get; set; }//Returns the price of the combi leg.
    }
}
