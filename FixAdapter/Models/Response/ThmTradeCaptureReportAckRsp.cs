//-----------------------------------------------------------------------------
// File Name   : ThmTradeCaptureReportAckRsp
// Author      : junlei
// Date        : 6/8/2020 9:16:54 AM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace FixAdapter.Models.Response {
    public class ThmTradeCaptureReportAckRsp {
        public string TradeRequestID { get; set; }
        public int TradeRequestType { get; set; }
        public int? TrdRptStatus { get; set; }
        public int? TradeRequestResult { get; set; }
        public string Text { get; set; }
        public List<ThmTradeSide> Sides { get; } = new List<ThmTradeSide>();
        public string TradeID { get; set; }
        public char? ExecType { get; set; }
        public int? TradeReportType { get; set; }
        public int? TradeReportTransType { get; set; }
        public string TradeReportID { get; set; }
        public int? TradeReportRejectReason { get; set; }
        public string ExecID { get; set; }
        public string Symbol { get; set; }

        public List<ThmLeg> Legs { get; } = new List<ThmLeg>();
        public int? PriceType { get; set; }
        public int? QtyType { get; set; }
        public decimal LastQty { get; set; }
        public decimal LastPx { get; set; }
        public char? MatchStatus { get; set; }
    }
}
