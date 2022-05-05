//-----------------------------------------------------------------------------
// File Name   : ThmSecurityDefinitionRsp
// Author      : junlei
// Date        : 6/10/2020 3:12:47 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace FixAdapter.Models.Response {
    public class ThmSecurityDefinitionRsp {
        public string SecurityReqID { get; set; }
        public int? SecurityResponseType { get; set; }
        public string Symbol { get; set; }
        public string SecurityType { get; set; }
        public string Text { get; set; }
        public string ProductComplex { get; set; }

        public List<ThmLeg> Legs { get; } = new List<ThmLeg>();
        public List<ThmMarketSegment> MarketSegments { get; } = new List<ThmMarketSegment>();
    }
}
