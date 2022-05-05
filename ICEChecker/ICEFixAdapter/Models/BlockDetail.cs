namespace ICEFixAdapter.Models {
    public class BlockDetail {
        public int BlockDetailsBlockType { get; set; }//Required with NoBlockDetails (Tag 9070) > ‘0’. 
                                                      //Supported value(s):
                                                      //0 = Regular
                                                      //1 = PNC
                                                      //2 = DP
                                                      //3 = LIS
        public string BlockDetailsTradeType { get; set; }
        public decimal BlockDetailsMinQty { get; set; }

    }
}
