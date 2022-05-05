//-----------------------------------------------------------------------------
// File Name   : ThmTradeCaptureReportRequestAckRsp
// Author      : junlei
// Date        : 6/16/2020 3:51:19 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace FixAdapter.Models.Response {
    public class ThmTradeCaptureReportRequestAckRsp {
        public string TradeRequestID { get; set; }
        public int TradeRequestType { get; set; }
        public int TradeRequestResult { get; set; }
        public int TradeRequestStatus { get; set; }
    }
}
