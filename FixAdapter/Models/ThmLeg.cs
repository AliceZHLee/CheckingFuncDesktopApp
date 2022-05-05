//-----------------------------------------------------------------------------
// File Name   : ThmLeg
// Author      : junlei
// Date        : 6/11/2020 2:51:00 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace FixAdapter.Models {
    public class ThmLeg {
        public string LegSymbol { get; set; }
        public string LegSecurityID { get; set; }
        public string LegSecurityIDSource { get; set; }
        public string LegSecurityType { get; set; }
        public string LegMaturityMonthYear { get; set; }
        public decimal? LegStrikePrice { get; set; }
        public int? LegPutOrCall { get; set; }  // 1358: 0 = Put; 1 = Call
        public int? LegExerciseStyle { get; set; }  // 0 = European; 1 = American; 2 = Bermuda
        public decimal? LegRatioQty { get; set; }
        public decimal? LegLastPx { get; set; }
        public decimal? LegLastQty { get; set; } //LegLastQty 1418
        public string LegReportID { get; set; }
    }
}
