//-----------------------------------------------------------------------------
// File Name   : IceTradeCaptureReportRequestAckRsp
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace ICEFixAdapter.Models.Response {
    public class IceTradeCaptureReportRequestAckRsp {//AQ
        public string TradeRequestID { get; set; }
        public int TradeRequestType { get; set; }//0=All Trades
        public int TradeRequestResult { get; set; }//0=Successful
                                                   //99=Others
        public int TradeRequestStatus { get; set; }//0 = Accepted,
                                                   //1 = Completed,
                                                   ///2 = Rejected
        public string CFICode { get; set; }
        public string SecurityID { get; set; }
        public List<Underlying> Underlyings = new List<Underlying>();
        public string Text { get; set; }
    }
}
