//-----------------------------------------------------------------------------
// File Name   : ThmMarketSegment
// Author      : junlei
// Date        : 7/1/2020 1:47:38 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System.Collections.Generic;

namespace FixAdapter.Models {
    public class ThmMarketSegment {
        public string MarketSegmentID { get; set; }
        public decimal LowLimitPrice { get; set; }
        public decimal HighLimitPrice { get; set; }
        public decimal TradingReferencePrice { get; set; }
        public int MultilegModel { get; set; }

        public List<TradingSessionRule> TradingSessionRules { get; } = new List<TradingSessionRule>();
    }

    public class TradingSessionRule {
        public string TradingSessionID { get; set; }
        public string TradingSessionSubID { get; set; }
    }
}
