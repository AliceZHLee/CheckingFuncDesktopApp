using System;
using System.Collections.Generic;

namespace ICEFixAdapter.Models.Response {
    public class IceAllocationReportRsp {
        public string AllocID { get; set; }
        public char AllocTransType { get; set; }//0 = New
                                                //1 = Replace
                                                //2 = Cancel(Bust)
                                                //6 = Reversal(custom value)
        public string ClearingBusinessDate { get; set; }
        public int AllocReportType { get; set; }
        public int AllocStatus { get; set; }
        public string TradeRequestID { get; set; }
        public List<string> CIOrdIDs { get; } = new List<string>();//always count=1
        public List<Execution> Execs { get; } = new List<Execution>();
        public char Side { get; set; }
        public string TrdType { get; set; }
        public int Symbol { get; set; }
        public string SecurityType { get; set; }//FUT = Futures
                                                //OPT = Options
        public string MaturityMonthYear { get; set; }
        public int PutOrCall { get; set; }
        public decimal StrikePrice { get; set; }
        public int OptionSymbol { get; set; }
        public string SecurityExchange { get; set; }
        public decimal Shares { get; set; }
        public decimal AvgPx { get; set; }
        public string TradeDate { get; set; }
        public DateTime TransactTime { get; set; }
        public string Text { get; set; }
        public string TradeInputSource { get; set; }
        public char LiquidityIndicator { get; set; }
        public List<Allocation> Allocs { get; } = new List<Allocation>();
    }
}
