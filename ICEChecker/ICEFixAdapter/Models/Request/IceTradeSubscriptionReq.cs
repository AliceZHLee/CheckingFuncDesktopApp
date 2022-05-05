//-----------------------------------------------------------------------------
// File Name   : IceTradeSubscriptionReq
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace ICEFixAdapter.Models.Request {
    public class IceTradeSubscriptionReq { // AD, sent to receive historical and real-time trades
        public string TradeReqID { get; set; }
        public int TradeRequestType { get; set; }
        public int SubscriptionRequestType { get; set; }//0 = Snapshot
                                                        //1 = Snapshot + Updates
                                                        //2 = Unsubscribe
        public List<IceParty> Parties = new List<IceParty>();
        public string CFICode { get; set; }
        public string SecurityID { get; set; }//if blank an dno underlying, then it means subscribe to all markets
        public List<Underlying> Underlyings= new List<Underlying>();
        public List<TradeTime> Dates=new List<TradeTime>(); 
        public int PublishClearingAllocations { get; set; }
    }
}
