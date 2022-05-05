//-----------------------------------------------------------------------------
// File Name   : ThmTradeSubscriptionReq
// Author      : junlei
// Date        : 6/11/2020 4:26:47 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace FixAdapter.Models.Request {
    public class ThmTradeSubscriptionReq { // AD
        public string TradeReqID { get; set; }
        public int TradeRequestType { get; set; }
        public char? SubscriptionRequestType { get; set; }
        public string TradeID { get; set; }
        public List<DateTime> Dates { get; } = new List<DateTime>();
    }
}
