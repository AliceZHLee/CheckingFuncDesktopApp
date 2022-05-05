//-----------------------------------------------------------------------------
// File Name   : ThmTradeCaptureReportReq
// Author      : junlei
// Date        : 6/10/2020 6:18:31 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace FixAdapter.Models.Request {
    public class ThmTradeCaptureReportReq { // AE
        public string TradeID { get; set; } //1003
        public string TradeReportID { get; set; }
        public int TradeReportTransType { get; set; } // 487
        public string Symbol { get; set; }
        public int TrdType { get; set; }
        public int? PriceType { get; set; }
        public int QtyType { get; set; }
        public decimal LastPx { get; set; }
        public decimal LastQty { get; set; }
        public DateTime? TransactTime { get; set; } = null;
        public string TradeDate { get; set; }
        public string Currency { get; set; }

        public int? TradeReportType { get; set; } = null;
        public string TradeRequestID { get; set; }
        public char? ExecType { get; set; } = null;
        public int? TotNumTradeReports { get; set; }
        public bool? LastRptRequested { get; set; } = null;
        public string ExecID { get; set; }
        public string ClearingBusinessDate { get; set; }
        public string MarketSegmentID { get; set; }

        public string SecurityID { get; set; }
        public string SecurityIDSource { get; set; }
        public string SecurityType { get; set; }
        public string MaturityMonthYear { get; set; }

        public List<ThmTradeSide> Sides { get; } = new List<ThmTradeSide>();

        public char? MultiLegReportingType { get; set; } = null;
        public List<ThmLeg> Legs { get; } = new List<ThmLeg>();
    }
}
