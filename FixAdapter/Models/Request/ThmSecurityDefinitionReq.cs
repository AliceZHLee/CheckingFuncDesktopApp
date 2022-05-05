//-----------------------------------------------------------------------------
// File Name   : ThmSecurityDefinitionReq
// Author      : junlei
// Date        : 6/11/2020 8:42:42 AM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace FixAdapter.Models.Request {
    public class ThmSecurityDefinitionReq {
        public string RequestID { get; set; }
        public int RequestType { get; set; }
        public string Symbol { get; set; } = "[N/A]";
        public string SecurityID { get; set; }
        public string SecurityType { get; set; }

        public List<ThmLeg> Legs { get; } = new List<ThmLeg>();
    }
}
