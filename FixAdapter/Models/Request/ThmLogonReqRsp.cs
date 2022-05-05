//-----------------------------------------------------------------------------
// File Name   : ThmLogonReqRsp
// Author      : junlei
// Date        : 6/11/2020 8:59:11 AM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace FixAdapter.Models.Request {
    public class ThmLogonReqRsp {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SessionStatus { get; set; }
        public string Text { get; set; }
        public string NewPassword { get; set; } = null; // if not null, then change the password
        public int MsgSeqNum { get; set; }
    }
}
