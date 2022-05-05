//-----------------------------------------------------------------------------
// File Name   : ThmMarketData
// Author      : junlei
// Date        : 5/27/2020 6:35:02 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace FixAdapter.Models.Response {
    public class ThmMarketDataRsp {
        public string MDReqID { get; set; }
        public string Symbol { get; set; }
        public string FailedResult { get; set; }

        public List<ThmMarketDataEntry> MDEntries { get; } = new List<ThmMarketDataEntry>();
    }

    public class ThmMarketDataEntry {
        /// <summary>
        /// 279 : 1 = Change; 2 = Delete
        /// </summary>
        public char? MDUpdateAction { get; set; }
        public char? MDEntryType { get; set; }
        public string Symbol { get; set; }
        public decimal MDEntryPx { get; set; }
        public string Currency { get; set; }

        public decimal MDEntrySize { get; set; }
        public DateTime MDEntryDate { get; set; }
        public DateTime MDEntryTime { get; set; }
        public string TradingSessionID { get; set; } // 1=Day
        public string TradingSessionSubID { get; set; }
        public string MDEntryOriginator { get; set; }
        public DateTime TransactTime { get; set; }
        public string SettlDate { get; set; }  // SGX Clear Date in YYYYMMDD format
    }

    public class ThmMarketDataSnapshotRsp {
        public string MDReqID { get; set; }
        public string Symbol { get; set; }

        public List<ThmMarketDataEntry> MDEntries { get; } = new List<ThmMarketDataEntry>();
    }

    public class ThmMarketDataIncrementalRsp {
        public List<ThmMarketDataEntry> MDEntries { get; } = new List<ThmMarketDataEntry>();
    }
}
