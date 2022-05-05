//-----------------------------------------------------------------------------
// File Name   : IceTradeSide
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace ICEFixAdapter.Models {
    public class IceTradeSide {
        public char Side { get; set; }  // 1=buy, 2=sell
        public int OrderID { get; set; }
        public string CIOrdID { get; set; }// Trade Capture Report for the strategy and the legs of the strategy will have the same ClOrdID (Tag 11)
        public List<IceParty> Parties { get; } = new List<IceParty>();
        public string ComplianceID { get; set; }
        public string CustOrderHandlingInst { get; set; }
        public char PositionEffect { get; set; }
        public string MemoField { get; set; }
        public string TransactDetails { get; set; }
        public int CrossExecutionType { get; set; }//0=basic crossing order
        public string Text { get; set; }
        public List<Allocation> Allocs { get; } = new List<Allocation>();    
    }
}
