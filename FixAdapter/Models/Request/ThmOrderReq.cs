//-----------------------------------------------------------------------------
// File Name   : ThmOrderReq
// Author      : junlei
// Date        : 6/12/2020 4:30:27 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace FixAdapter.Models.Request {
    public class ThmOrderReq {
        public string OrderID { get; set; } // 37
        public string Account { get; set; }
        public string ClOrdID { get; set; }
        public string Symbol { get; set; } // 55
        public decimal Price { get; set; }
        public int OrderQty { get; set; }
        public char? Side { get; set; } = null; // 1 buy, 2 sell
        public char? OrdType { get; set; } = null;
        public char? TimeInForce { get; set; } = null;
        public DateTime TransactTime { get; set; }
        public decimal StopPx { get; set; }
        public char? HandlInst { get; set; } = null;

        /// <summary>
        /// 530
        /// </summary>
        public char? MassCancelRequestType { get; set; } 
        public string Text { get; set; }
        public string ExecInst { get; set; }
        public string OrdStatusReqID { get; set; } // 790

        public List<ThmParty> Parties { get; } = new List<ThmParty>();

        public List<ThmTargetParty> TargetParties { get; } = new List<ThmTargetParty>();
        public List<ThmValueCheck> ValueChecks { get; } = new List<ThmValueCheck>();
        public int? ExposureDuration { get; set; }
        public string RefOrderID { get; set; }
        public char? RefOrderIDSource { get; set; }
        public string OrigClOrdID { get; set; }

        public MatchGroup MatchGroup { get; set; }
    }

    public class MatchGroup {
        public int MatchInst { get; set; }  // 1625
        public string MatchAttribTagID { get; set; } // 1626
        public string MatchAttribValue { get; set; } // 1627
    }
}
