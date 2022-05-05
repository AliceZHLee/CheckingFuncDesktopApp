//-----------------------------------------------------------------------------
// File Name   : ThmErrorRsp
// Author      : junlei
// Date        : 6/29/2020 2:52:26 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace FixAdapter.Models.Response {
    public class ThmErrorRsp {
        public string MsgSeqNum { get; set; }
        public string Text { get; set; }
        public string Message { get; set; }
        /// <summary>
        /// 1 = Session Password Changed
        /// 2 = Session password due to expire
        /// 3 = New Session Password Does Not Comply With Policy
        /// 4 = Session Logout Complete
        /// 5 = Invalid username or password
        /// 6 = Account locked
        /// </summary>
        public string SessionStatus { get; set; }
    }
}
