//-----------------------------------------------------------------------------
// File Name   : IceErrorRsp
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace ICEFixAdapter.Models.Response {
    public class IceErrorRsp {
        public string MsgSeqNum { get; set; }//34
        public int RefTagID { get; set; }//371
        public string RefMsgType { get; set; }//372: MsgType of the FIX message being referenced
        public string Text { get; set; }//58
        public string Message { get; set; }
        public int SessionRejectReason { get; set; }
        public int RefSeqNum { get; set; }
    }
}
