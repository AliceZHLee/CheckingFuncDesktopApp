//-----------------------------------------------------------------------------
// File Name   : ThmSecurityRsp
// Author      : junlei
// Date        : 5/27/2020 6:38:47 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace FixAdapter.Models.Response {
    public class ThmSecurityListRsp {
        public string SecurityReqID { get; set; } // 320
        public string SecurityResponseID { get; set; } // 322
        public int SecurityRequestResult { get; set; } //560

        public List<ThmSecurity> Securities { get; } = new List<ThmSecurity>();
    }
}
