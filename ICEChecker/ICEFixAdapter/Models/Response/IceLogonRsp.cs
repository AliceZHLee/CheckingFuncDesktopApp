namespace ICEFixAdapter.Models.Response {
    public class IceLogonRsp {        
        public string EncrytMethod { get; set; }//0=Clear text
        public int HeartBtInt { get; set; }//default=30 sec
        public string Text { get; set; }
        public int MsgSeqNum { get; set; }
    }
}
