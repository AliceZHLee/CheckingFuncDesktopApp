//-----------------------------------------------------------------------------
// File Name   : ThmOrderMassCancelReportRsp
// Author      : junlei
// Date        : 6/16/2020 5:14:55 PM
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace FixAdapter.Models.Response {
    public class ThmOrderMassCancelReportRsp {
        public string ClOrdID { get; set; }
        public char MassCancelRequestType { get; set; }
        public char? MassCancelResponse { get; set; }
        public int? MassCancelRejectReason { get; set; }
        public string Text { get; set; }
        public string MassActionReportID { get; set; }
    }
}
