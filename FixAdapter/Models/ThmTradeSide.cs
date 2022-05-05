//-----------------------------------------------------------------------------
// File Name   : ThmTradeSide
// Author      : junlei
// Date        : 6/11/2020 11:06:44 AM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

using System.Collections.Generic;

namespace FixAdapter.Models {
    public class ThmTradeSide {
        public char? Side { get; set; }  // buy 1, sell 2
        public string SideTradeReportID { get; set; }
        public string Account { get; set; }
        public string ExchangeSpecialInstructions { get; set; }

        public string Text { get; set; }
        public bool? AggressorIndicator { get; set; } = null;
        public string TradeInputSource { get; set; }
        public string SideExecID { get; set; }
        public string TradingSessionID { get; set; }
        public string TradingSessionSubID { get; set; }

        public List<ThmParty> Parties { get; } = new List<ThmParty>();
        public int? SideRiskLimitCheckStatus { get; set; } = null;
    }
}
