//-----------------------------------------------------------------------------
// File Name   : ThmTradeCaptureReport
// Author      : junlei
// Date        : 5/27/2020 6:03:03 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace FixAdapter.Models.Response {
    public class ThmTradeCaptureReportRsp {
        public string TradeID { get; set; }
        public string ExecID { get; set; }
        public char ExecType { get; set; }
        public string TradeDate { get; set; }
        public string Message { get; set; }
        public decimal LastQty { get; set; }
        public decimal LastPx { get; set; }
        public int QtyType { get; set; }
        public DateTime TransactTime { get; set; }

        public string SecurityID { get; set; }
        public string Symbol { get; set; }
        public int TotNumTradeReports { get; set; }
        public bool LastRptRequested { get; set; }

        public string TradeReportID { get; set; }
        public int TradeReportTransType { get; set; }
        public int TradeReportType { get; set; }
        public string TradeRequestID { get; set; }
        public int TrdType { get; set; }
        public int PriceType { get; set; }
        public string ClearingBusinessDate { get; set; }

        public List<ThmTradeSide> Sides { get; } = new List<ThmTradeSide>();
        public string Currency { get; set; }
        public string MarketSegmentID { get; set; }
        public int? RiskLimitCheckStatus { get; set; } = null;
    }
}
