//-----------------------------------------------------------------------------
// File Name   : IceLogonReqRsp
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace ICEFixAdapter.Models.Request {
    public class IceLogonReqRsp {
        public string UserName { get; set; }//"theme-dcfx"
        public string Password { get; set; }//"Starts*123"
        public int EncryptMethod { get; set; }//0=Clear text
        public int HeartBtInt { get; set; }//default=30 sec
        public bool ResetSeqNumFlag { get; set; }
        public int MsgSeqNum { get; set; }
        public string Text { get; set; }
    }
}
