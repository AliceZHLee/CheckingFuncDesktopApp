//-----------------------------------------------------------------------------
// File Name   : IceSecurity
// Author      : Alice Li
// Date        : 2/28/2022 
// Description : 
// Version     : 1.0.0      
// Updated     : 
//
//-----------------------------------------------------------------------------

namespace ICEFixAdapter.Models {
    public class IceSecurity {
        public string Symbol { get; set; } // 55
        public string SecurityID { get; set; } // 48
        public string SecurityIDSource { get; set; } //22
        public string SecurityDesc { get; set; } // 107
        public string SecurityGroup { get; set; } // 1151
    }
}
