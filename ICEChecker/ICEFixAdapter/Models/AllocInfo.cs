namespace ICEFixAdapter.Models {
    public class AllocInfo {
        public int AllocSideInfo { get; set; }//1=give up
                                              //2=take up
        public int ClientID { get; set; }
        public int BrokerCompID { get; set; }
        public string OriginatorUserID { get; set; }
        public char AccountCode { get; set; }

        public string TMMnemonic { get; set; }
        public string RIMnemonic { get; set; }
        public string CMMnemonic { get; set; }
        public string ClrHouseCode { get; set; }
        public string CustomerAccountReflID { get; set; }
        public int CTICode { get; set; }
    }
}
