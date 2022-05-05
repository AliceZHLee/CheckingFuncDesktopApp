//-----------------------------------------------------------------------------
// File Name   : ThmPartyDetailsRsp
// Author      : junlei
// Date        : 6/9/2020 9:53:20 AM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System.Collections.Generic;

namespace FixAdapter.Models.Response {
    public class ThmPartyDetailsRsp {
        public string PartyDetailsListReportID { get; set; }
        public string PartyDetailsListReqeustID { get; set; }
        public int? PartyDetailsRequestResult { get; set; }
        public List<ThmParty> Parties { get; } = new List<ThmParty>();
    }
}
