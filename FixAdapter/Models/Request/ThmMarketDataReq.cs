//-----------------------------------------------------------------------------
// File Name   : ThmMarketDataReq
// Author      : junlei
// Date        : 6/12/2020 9:18:34 AM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace FixAdapter.Models.Request {
    public class ThmMarketDataReq {
        public string MdReqId { get; set; }
        /// <summary>
        /// 0 = Snapshot
        /// 1 = Snapshot + Updates(Subscribe)
        /// 2 = Disable previous Snapshot + Update Request(Unsubscribe)
        /// </summary>
        public char? SubscriptionRequestType { get; set; }
        public int MarketDepth { get; set; } = 0;
        public string Exchange { get; set; }

        public List<ThmMDEntry> MDEntries { get; } = new List<ThmMDEntry>();
        public List<ThmSymbol> RelatedSymbols { get; } = new List<ThmSymbol>();
    }

    public class ThmMDEntry {
        public char? EntryType { get; set; }
    }

    public class ThmSymbol {
        public string Symbol { get; set; }
    }
}
