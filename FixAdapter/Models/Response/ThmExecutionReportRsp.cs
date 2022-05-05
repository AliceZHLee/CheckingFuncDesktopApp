//-----------------------------------------------------------------------------
// File Name   : ThmExecutionReportRsp
// Author      : junlei
// Date        : 6/12/2020 12:08:07 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace FixAdapter.Models.Response {
    public class ThmExecutionReportRsp {
        public char? OrdStatus { get; set; } = null;
        public char? ExecType { get; set; } = null;
        public decimal? LeavesQty { get; set; }

        public string OrderID { get; set; }
        public string SecondaryOrderID { get; set; }
        public string SecondaryExecID { get; set; }
        public string ClOrdID { get; set; }
        public string ExecID { get; set; }
        public string OrigClOrdID { get; set; }

        public List<ThmParty> Parties { get; } = new List<ThmParty>();
        public char? OrderCategory { get; set; }
        public string Account { get; set; }
        public string ExecInst { get; set; }
        public string Symbol { get; set; }
        public char? Side { get; set; }
        public decimal? OrderQty { get; set; }
        public char? OrdType { get; set; }
        public decimal? Price { get; set; }
        public char? TimeInForce { get; set; }
        public object ExposureDuration { get; set; }
        public decimal? CumQty { get; set; }
        public decimal? LastPx { get; set; }
        public DateTime? TransactTime { get; set; }
        public string Text { get; set; }

        public List<ThmValueCheck> ValueChecks { get; } = new List<ThmValueCheck>();
        public List<ThmTargetParty> TargetParties { get; } = new List<ThmTargetParty>();
    }
}
