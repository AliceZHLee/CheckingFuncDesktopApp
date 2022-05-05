using System;

namespace ICEFixAdapter.Models {
    public class TradeTime {// request can select trade dates to see the specific sessions' trades
        public DateTime TradeDate { get; set; }//local mkt date
        public DateTime TransactTime { get; set; }//UTC timestamp
    }
}
