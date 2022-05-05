//-----------------------------------------------------------------------------
// File Name   : IceTradeCaptureReportAckRsp
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace ICEFixAdapter.Models.Response {
    public class IceTradeCaptureReportAckRsp {
        public string TradeRequestID { get; set; }
        public int TradeRequestType { get; set; }
        public int? TrdRptStatus { get; set; }
        public int? TradeRequestResult { get; set; }
        public string Text { get; set; }
        public List<IceTradeSide> Sides { get; } = new List<IceTradeSide>();
        public string TradeID { get; set; }
        public char? ExecType { get; set; }
        public int? TradeReportType { get; set; }
        public int? TradeReportTransType { get; set; }
        public string TradeReportID { get; set; }
        public int? TradeReportRejectReason { get; set; }
        public string ExecID { get; set; }
        public string Symbol { get; set; }

        public List<IceLeg> Legs { get; } = new List<IceLeg>();
        public int? PriceType { get; set; }
        public int? QtyType { get; set; }
        public decimal LastQty { get; set; }
        public decimal LastPx { get; set; }
        public char? MatchStatus { get; set; }
    }
}
