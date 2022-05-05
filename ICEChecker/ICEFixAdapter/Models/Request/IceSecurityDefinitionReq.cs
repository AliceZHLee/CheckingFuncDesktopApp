//-----------------------------------------------------------------------------
// File Name   : IceSecurityDefinitionReq
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace ICEFixAdapter.Models.Request {
    public class IceSecurityDefinitionReq {//35=c
        public string SecurityReqID { get; set; }//unique of request ID
        public int SecurityRequestType { get; set; }//Y,
                                                    //3=Request list of securities(more often used),
                                                    //101=Request list of defined strategies
        public string SecurityID { get; set; }//market type name / market type id, refer to "support_market_types_on_ice_api.pdf
                                              //2=Oil
                                              //165=Oil Americas
        public string CFICode { get; set; }//required if SecurityRequestType='3'
    }
}
