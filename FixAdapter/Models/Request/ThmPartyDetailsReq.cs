//-----------------------------------------------------------------------------
// File Name   : ThmPartyDetailsReq
// Author      : junlei
// Date        : 6/15/2020 9:16:55 AM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------
using System.Collections.Generic;

namespace FixAdapter.Models.Request {
    public class ThmPartyDetailsReq {
        public string ReqID { get; set; }
        public List<int> PartyListResponseTypes { get; } = new List<int>();
    }
}
